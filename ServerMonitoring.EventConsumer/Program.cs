using ServerMonitoring.SignalR.DependencyInjection;

namespace ServerMonitoring.EventConsumer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Services.AddSignalRNotificationListener(builder.Configuration);
        builder.Services.AddHostedService<AlertListenerWorker>();

        var host = builder.Build();
        host.Run();
    }
}