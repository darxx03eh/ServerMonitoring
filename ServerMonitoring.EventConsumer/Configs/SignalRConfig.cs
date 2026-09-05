namespace ServerMonitoring.EventConsumer.Configs;

public class SignalRConfig
{
    public string SignalRUrl { get; set; } = "http://localhost:8080/hubs/alerts";
}