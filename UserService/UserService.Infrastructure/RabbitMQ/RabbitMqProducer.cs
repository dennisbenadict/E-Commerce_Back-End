using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.RabbitMQ;

public class RabbitMqProducer : IEventProducer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly object _lock = new();

    public RabbitMqProducer(string hostName, int port = 5672, string user = "guest", string pass = "guest")
    {
        var factory = new ConnectionFactory()
        {
            HostName = hostName,
            Port = port,
            UserName = user,
            Password = pass
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    private void EnsureExchange(string exchangeName)
    {
        lock (_lock)
        {
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: true);
        }
    }

    public Task PublishAsync(string exchangeName, object message)
    {
        EnsureExchange(exchangeName);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = _channel.CreateBasicProperties();
        props.DeliveryMode = 2; // persistent

        _channel.BasicPublish(
            exchange: exchangeName,
            routingKey: "",
            basicProperties: props,
            body: body
        );

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch { }
    }
}


