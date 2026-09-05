using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerMonitoring.EventConsumer.Configs;
using ServerMonitoring.EventConsumer.Services;

namespace ServerMonitoring.EventConsumer;

class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.Configure<SignalRConfig>(
            builder.Configuration.GetSection("SignalRConfig"));

        builder.Services.AddHostedService<SignalRListenerService>();

        var host = builder.Build();
        await host.RunAsync();
    }
}