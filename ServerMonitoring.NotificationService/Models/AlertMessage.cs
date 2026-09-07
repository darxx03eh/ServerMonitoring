namespace ServerMonitoring.NotificationService.Models;

public class AlertMessage
{
    public string AlertType { get; set; } = string.Empty;
    public string ServerIdentifier { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}