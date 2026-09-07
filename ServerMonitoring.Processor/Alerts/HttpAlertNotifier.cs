using System.Net.Http.Json;
using ServerMonitoring.Processor.Models;

namespace ServerMonitoring.Processor.Alerts;

public class HttpAlertNotifier(
    HttpClient httpClient,
    ILogger<HttpAlertNotifier> logger
    ) : IAlertNotifier
{
    public async Task SendAlertAsync(AlertMessage alert, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/alerts", alert, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send alert to NotificationService.");
        }
    }
}