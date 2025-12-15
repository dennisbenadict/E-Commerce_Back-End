namespace AuthService.Application.Interfaces;

public interface IEventProducer
{
    Task PublishAsync(string exchangeName, object message);
}

