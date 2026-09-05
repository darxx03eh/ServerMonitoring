using Microsoft.AspNetCore.SignalR;
using ServerMonitoring.Processor.Alerts.Hubs;
using ServerMonitoring.Processor.Models;

namespace ServerMonitoring.Processor.Alerts;

public class SignalRAlertNotifier : IAlertNotifier
{
    private readonly IHubContext<AlertsHub> _hubContext;

    public SignalRAlertNotifier(IHubContext<AlertsHub> hubContext) => _hubContext = hubContext;

    public Task SendAlertAsync(AlertMessage alert, CancellationToken cancellationToken = default)
        => _hubContext.Clients.All.SendAsync(alert.AlertType, alert, cancellationToken);
}