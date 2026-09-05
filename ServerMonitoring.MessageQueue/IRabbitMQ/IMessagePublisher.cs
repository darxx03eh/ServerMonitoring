namespace ServerMonitoring.MessageQueue.IRabbitMQ;

public interface IMessagePublisher : IAsyncDisposable
{
    Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default);
}