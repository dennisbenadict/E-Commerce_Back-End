using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace UserService.Infrastructure.RabbitMQ;

public class RabbitMqProducer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqProducer(string hostName, int port = 5672, string user = "guest", string pass = "guest")
    {
        var factory = new ConnectionFactory() { HostName = hostName, Port = port, UserName = user, Password = pass };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void EnsureExchange(string exchangeName)
    {
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: true);
    }

    public void Publish<T>(string exchangeName, T message)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        _channel.BasicPublish(exchange: exchangeName, routingKey: "", basicProperties: null, body);
    }

    public void Dispose()
    {
        try { _channel?.Close(); _connection?.Close(); } catch { }
    }
}
