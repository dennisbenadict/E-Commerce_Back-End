using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using AuthService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Api.Consumers;

public class UserProfileUpdatedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserProfileUpdatedConsumer> _logger;
    private readonly string _host;
    private readonly string _user;
    private readonly string _password;

    private IConnection? _connection;
    private IModel? _channel;

    private const string ExchangeNameUpdated = "user.profile.updated";
    private const string ExchangeNameCreated = "user.profile.created";
    private const string QueueNameUpdated = "auth-service.user.profile.updated";
    private const string QueueNameCreated = "auth-service.user.profile.created";

    public UserProfileUpdatedConsumer(
        IServiceProvider serviceProvider,
        ILogger<UserProfileUpdatedConsumer> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _host = configuration["RabbitMq:Host"] ?? "rabbitmq";
        _user = configuration["RabbitMq:User"] ?? "guest";
        _password = configuration["RabbitMq:Password"] ?? "guest";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _host,
                    UserName = _user,
                    Password = _password,
                    DispatchConsumersAsync = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Set up both created and updated exchanges
                _channel.ExchangeDeclare(ExchangeNameCreated, ExchangeType.Fanout, durable: true);
                _channel.ExchangeDeclare(ExchangeNameUpdated, ExchangeType.Fanout, durable: true);
                
                _channel.QueueDeclare(QueueNameCreated, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueDeclare(QueueNameUpdated, durable: true, exclusive: false, autoDelete: false);
                
                _channel.QueueBind(QueueNameCreated, ExchangeNameCreated, "");
                _channel.QueueBind(QueueNameUpdated, ExchangeNameUpdated, "");

                _logger.LogInformation("UserProfileUpdatedConsumer connected. Listening on exchanges: {ExchangeCreated}, {ExchangeUpdated}", 
                    ExchangeNameCreated, ExchangeNameUpdated);

                // Consumer for profile.created
                var consumerCreated = new AsyncEventingBasicConsumer(_channel);
                consumerCreated.Received += async (_, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var message = JsonSerializer.Deserialize<UserProfileUpdatedEvent>(json);

                        if (message == null)
                        {
                            _logger.LogWarning("Invalid user.profile.created message received");
                            return;
                        }

                        _logger.LogInformation("Profile created event received for UserId: {UserId}", message.UserId);
                        await SyncProfileToAuthService(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing user.profile.created event");
                    }
                };

                // Consumer for profile.updated
                var consumerUpdated = new AsyncEventingBasicConsumer(_channel);
                consumerUpdated.Received += async (_, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var message = JsonSerializer.Deserialize<UserProfileUpdatedEvent>(json);

                        if (message == null)
                        {
                            _logger.LogWarning("Invalid user.profile.updated message received");
                            return;
                        }

                        _logger.LogInformation("Profile updated event received for UserId: {UserId}", message.UserId);
                        await SyncProfileToAuthService(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing user.profile.updated event");
                    }
                };

                _channel.BasicConsume(QueueNameCreated, autoAck: true, consumer: consumerCreated);
                _channel.BasicConsume(QueueNameUpdated, autoAck: true, consumer: consumerUpdated);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ not ready. Retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task SyncProfileToAuthService(UserProfileUpdatedEvent message)
    {
        using var scope = _serviceProvider.CreateScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Sync profile data from UserService to AuthService
        var user = await userRepo.GetByIdAsync(message.UserId);
        if (user != null)
        {
            user.Name = message.Name ?? user.Name;
            user.Email = message.Email ?? user.Email;
            user.Phone = message.Phone ?? user.Phone;
            
            await userRepo.UpdateUserAsync(user);
            await userRepo.SaveChangesAsync();
            
            _logger.LogInformation("User profile synced to AuthService for UserId {UserId}", message.UserId);
        }
        else
        {
            _logger.LogWarning("User {UserId} not found in AuthService for profile sync", message.UserId);
        }
    }

    public override void Dispose()
    {
        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch { }

        base.Dispose();
    }
}

public class UserProfileUpdatedEvent
{
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime Timestamp { get; set; }
}
