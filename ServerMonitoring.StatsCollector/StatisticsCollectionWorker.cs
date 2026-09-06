using Microsoft.Extensions.Options;
using ServerMonitoring.MessageQueue.IRabbitMQ;
using ServerMonitoring.StatsCollector.IProviders;
using ServerMonitoring.StatsCollector.Options;

namespace ServerMonitoring.StatsCollector;

public class StatisticsCollectionWorker(
    IServerStatisticsProvider statisticsProvider,
    IMessagePublisher publisher,
    IOptions<ServerStatisticsOptions> options,
    ILogger<StatisticsCollectionWorker> logger): BackgroundService
{
    private readonly ServerStatisticsOptions _options = options.Value;
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.SamplingIntervalSeconds);
        var topic = $"ServerStatistics.{_options.ServerIdentifier}";

        logger.LogInformation(
            "Starting statistics collection for '{ServerIdentifier}' every {Interval}s, publishing to '{Topic}'.",
            _options.ServerIdentifier, _options.SamplingIntervalSeconds, topic);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var stats = statisticsProvider.GetCurrentStatistics(_options.ServerIdentifier);
                await publisher.PublishAsync(topic, stats, stoppingToken);

                logger.LogInformation(
                    "Published stats: CPU={Cpu:F1}%, MemUsed={Mem:F0}MB, MemAvailable={Avail:F0}MB",
                    stats.CpuUsage, stats.MemoryUsage, stats.AvailableMemory);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to collect or publish server statistics.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}