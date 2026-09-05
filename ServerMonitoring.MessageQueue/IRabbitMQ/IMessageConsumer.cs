namespace ServerMonitoring.MessageQueue.IRabbitMQ;

public interface IMessageConsumer : IAsyncDisposable
{
    Task SubscribeAsync<T>(string topicPattern, Func<T, string, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default);
}