using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using AuthService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Api.Consumers;

public class UserPasswordChangedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserPasswordChangedConsumer> _logger;
    private readonly string _host;
    private readonly string _user;
    private readonly string _password;

    private IConnection? _connection;
    private IModel? _channel;

    private const string ExchangeName = "user.password.changed";
    private const string QueueName = "auth-service.user.password.changed";

    public UserPasswordChangedConsumer(
        IServiceProvider serviceProvider,
        ILogger<UserPasswordChangedConsumer> logger,
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

                _logger.LogInformation("UserPasswordChangedConsumer connected. Listening on exchange: {ExchangeName}", ExchangeName);

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.Received += async (_, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var message = JsonSerializer.Deserialize<UserPasswordChangedEvent>(json);

                        if (message == null)
                        {
                            _logger.LogWarning("Invalid user.password.changed message received");
                            return;
                        }

                        _logger.LogInformation("Password changed event received for UserId: {UserId}", message.UserId);

                        using var scope = _serviceProvider.CreateScope();
                        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                        // Sync password hash from UserService to AuthService
                        if (!string.IsNullOrEmpty(message.PasswordHash))
                        {
                            var user = await userRepo.GetByIdAsync(message.UserId);
                            if (user != null)
                            {
                                user.PasswordHash = message.PasswordHash;
                                await userRepo.SaveChangesAsync();
                                _logger.LogInformation("Password hash synced for UserId {UserId} in AuthService", message.UserId);
                            }
                            else
                            {
                                _logger.LogWarning("User {UserId} not found in AuthService for password sync", message.UserId);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Password hash not provided in password changed event for UserId {UserId}", message.UserId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing user.password.changed event");
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

public class UserPasswordChangedEvent
{
    public int UserId { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime Timestamp { get; set; }
}

