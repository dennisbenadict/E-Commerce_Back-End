namespace UserService.Application.Interfaces;

public interface IEventProducer
{
    Task PublishAsync(string exchangeName, object message);
}
