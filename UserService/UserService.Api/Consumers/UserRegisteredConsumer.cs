using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UserService.Application.Interfaces;
using UserService.Application.DTOs;

namespace UserService.Api.Consumers;

public class UserRegisteredConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRegisteredConsumer> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    private const string QueueName = "user.registered";

    public UserRegisteredConsumer(IServiceProvider serviceProvider, ILogger<UserRegisteredConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);

        _logger.LogInformation("UserRegisteredConsumer initialized. Listening on queue: {QueueName}", QueueName);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<UserRegisteredEvent>(json);

                if (message == null)
                {
                    _logger.LogWarning("Received empty or invalid message on 'user.registered'");
                    return;
                }

                _logger.LogInformation("Received UserRegistered event for UserId: {UserId}", message.UserId);

                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IUserProfileService>();

                await service.UpdateProfileAsync(message.UserId, new UpdateProfileDto
                {
                    Name = message.Name ?? "",
                    Email = message.Email ?? "",
                    Phone = message.Phone ?? ""
                });

                _logger.LogInformation("UserProfile upsert completed for UserId: {UserId}", message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process UserRegistered event");
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);

        _logger.LogInformation("UserRegisteredConsumer started consuming");

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        try
        {
            _channel.Close();
            _connection.Close();
        }
        catch { }

        base.Dispose();
    }
}

public class UserRegisteredEvent
{
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

