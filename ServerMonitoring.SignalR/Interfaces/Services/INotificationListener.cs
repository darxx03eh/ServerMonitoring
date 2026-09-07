namespace ServerMonitoring.SignalR.Interfaces.Services;

public interface INotificationListener : IAsyncDisposable
{
    Task SubscribeAsync<T>(string @event, Func<T, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default);

    Task StartAsync(CancellationToken cancellationToken = default);
}