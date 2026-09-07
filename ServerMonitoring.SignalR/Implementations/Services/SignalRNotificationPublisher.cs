using Microsoft.AspNetCore.SignalR;
using ServerMonitoring.SignalR.Hubs;
using ServerMonitoring.SignalR.Interfaces.Services;

namespace ServerMonitoring.SignalR.Implementations.Services;

public class SignalRNotificationPublisher(IHubContext<NotificationsHub> hubContext) : INotificationPublisher
{
    public Task PublishAsync<T>(string @event, T payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.All.SendAsync(@event, payload, cancellationToken);
}