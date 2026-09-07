using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerMonitoring.SignalR.Implementations.Services;
using ServerMonitoring.SignalR.Interfaces.Services;
using ServerMonitoring.SignalR.Options;

namespace ServerMonitoring.SignalR.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSignalRHubHosting()
        {
            services.AddSignalR();
            services.AddSingleton<INotificationPublisher, SignalRNotificationPublisher>();
            return services;
        }

        public IServiceCollection AddSignalRNotificationListener(IConfiguration configuration, string sectionName = "SignalR")
        {
            services.Configure<SignalRListenerOptions>(configuration.GetSection(sectionName));
            services.AddSingleton<INotificationListener, SignalRNotificationListener>();
            return services;
        }
    }
}