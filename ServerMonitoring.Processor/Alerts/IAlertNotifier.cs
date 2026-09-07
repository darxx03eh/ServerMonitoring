using ServerMonitoring.Processor.Models;

namespace ServerMonitoring.Processor.Alerts;

public interface IAlertNotifier
{ 
    Task SendAlertAsync(AlertMessage alert, CancellationToken cancellationToken = default);
}