using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UserService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace UserService.Api.Consumers;

public class UserDataSyncedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserDataSyncedConsumer> _logger;
    private readonly string _host;
    private readonly string _user;
    private readonly string _password;

    private IConnection? _connection;
    private IModel? _channel;

    private const string ExchangeName = "user.data.synced";
    private const string QueueName = "user-service.user.data.synced";

    public UserDataSyncedConsumer(
        IServiceProvider serviceProvider,
        ILogger<UserDataSyncedConsumer> logger,
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

                _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);
                _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(QueueName, ExchangeName, "");

                _logger.LogInformation("UserDataSyncedConsumer connected. Listening on exchange: {ExchangeName}", ExchangeName);

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.Received += async (_, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var message = JsonSerializer.Deserialize<UserDataSyncedEvent>(json);

                        if (message == null)
                        {
                            _logger.LogWarning("Invalid user.data.synced message received");
                            return;
                        }

                        _logger.LogInformation("User data synced event received for UserId: {UserId}", message.UserId);

                        using var scope = _serviceProvider.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IUserProfileRepository>();

                        // Update profile with synced data
                        var profile = await repo.GetByIdAsync(message.UserId);
                        if (profile != null)
                        {
                            // Only update if current values are empty
                            if (string.IsNullOrWhiteSpace(profile.Name))
                                profile.Name = message.Name ?? "";
                            if (string.IsNullOrWhiteSpace(profile.Email))
                                profile.Email = message.Email ?? "";
                            if (string.IsNullOrWhiteSpace(profile.Phone))
                                profile.Phone = message.Phone ?? "";

                            await repo.UpdateAsync(profile);
                            await repo.SaveChangesAsync();
                            
                            _logger.LogInformation("Profile updated with synced data for UserId {UserId}", message.UserId);
                        }
                        else
                        {
                            _logger.LogWarning("Profile not found for UserId {UserId} during sync", message.UserId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing user.data.synced event");
                    }
                };

                _channel.BasicConsume(QueueName, autoAck: true, consumer: consumer);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ not ready. Retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
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

public class UserDataSyncedEvent
{
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime Timestamp { get; set; }
}

