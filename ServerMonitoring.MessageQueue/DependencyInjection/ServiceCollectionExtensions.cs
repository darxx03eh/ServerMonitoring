using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ServerMonitoring.MessageQueue.IRabbitMQ;
using ServerMonitoring.MessageQueue.Options;
using ServerMonitoring.MessageQueue.RabbitMQ;

namespace ServerMonitoring.MessageQueue.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddRabbitMqMessaging(IConfiguration configuration, string sectionName = "RabbitMq")
        {
            services.Configure<RabbitMqOptions>(configuration.GetSection(sectionName));
            
            services.AddSingleton<IMessagePublisher>(sp =>
                new RabbitMqPublisher(sp.GetRequiredService<IOptions<RabbitMqOptions>>()));

            services.AddSingleton<IMessageConsumer>(sp =>
                new RabbitMqConsumer(sp.GetRequiredService<IOptions<RabbitMqOptions>>()));
            return services;
        }
    }
}