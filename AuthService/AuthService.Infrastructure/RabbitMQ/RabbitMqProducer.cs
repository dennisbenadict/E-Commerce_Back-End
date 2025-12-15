using System.Text;
using System.Text.Json;
using AuthService.Application.Interfaces;
using RabbitMQ.Client;

namespace AuthService.Infrastructure.RabbitMQ;

public class RabbitMqProducer : IEventProducer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqProducer(string hostName, string userName, string password)
    {
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public Task PublishAsync(string exchangeName, object message)
    {
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: true);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;

        _channel.BasicPublish(
            exchange: exchangeName,
            routingKey: string.Empty,
            basicProperties: props,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch
        {
            // swallow disposal exceptions
        }
    }
}

