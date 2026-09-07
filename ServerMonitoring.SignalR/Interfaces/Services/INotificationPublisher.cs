namespace ServerMonitoring.SignalR.Interfaces.Services;

public interface INotificationPublisher
{
    Task PublishAsync<T>(string @event, T payload, CancellationToken cancellationToken = default);
}