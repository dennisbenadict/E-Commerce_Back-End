using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.RabbitMQ;

public class RabbitMqProducer : IEventProducer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqProducer(string hostName)
    {
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public Task PublishAsync(string exchangeName, object message)
    {
        _channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, durable: true);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;

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
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch { }
    }
}
