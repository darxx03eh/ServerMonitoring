namespace ServerMonitoring.SignalR.Options;

public class SignalRListenerOptions
{
    public string HubUrl { get; set; } = string.Empty;
    public int RetryDelaySeconds { get; set; } = 5;
}