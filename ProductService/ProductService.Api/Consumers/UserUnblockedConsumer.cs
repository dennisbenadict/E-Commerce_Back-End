using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

namespace ProductService.Api.Consumers;

public class UserUnblockedConsumer : BackgroundService
{
    private readonly ILogger<UserUnblockedConsumer> _logger;
    private readonly string _host;
    private readonly string _user;
    private readonly string _password;

    private IConnection? _connection;
    private IModel? _channel;

    private const string ExchangeName = "user.unblocked";
    private const string QueueName = "product-service.user.unblocked";

    public UserUnblockedConsumer(
        ILogger<UserUnblockedConsumer> logger,
        IConfiguration configuration)
    {
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

                _logger.LogInformation("UserUnblockedConsumer connected. Listening on exchange: {ExchangeName}", ExchangeName);

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.Received += async (_, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var message = JsonSerializer.Deserialize<UserUnblockedEvent>(json);

                        if (message == null)
                        {
                            _logger.LogWarning("Invalid user.unblocked message received");
                            return;
                        }

                        _logger.LogInformation("User unblocked event received for UserId: {UserId}", message.UserId);
                        // User can now access cart/orders again - no action needed, just log
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing user.unblocked event");
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

public class UserUnblockedEvent
{
    public int UserId { get; set; }
    public DateTime Timestamp { get; set; }
}

