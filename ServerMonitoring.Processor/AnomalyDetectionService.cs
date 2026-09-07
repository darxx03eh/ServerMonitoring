using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using ServerMonitoring.MessageQueue.IRabbitMQ;
using ServerMonitoring.Processor.Alerts;
using ServerMonitoring.Processor.Interfaces.Repositories;
using ServerMonitoring.Processor.Models;
using ServerMonitoring.Processor.Options;

namespace ServerMonitoring.Processor;

public class AnomalyDetectionService(
    IMessageConsumer consumer,
    IServerStatisticsRepository repository,
    IAlertNotifier alertNotifier,
    IOptions<AnomalyDetectionOptions> options,
    ILogger<AnomalyDetectionService> logger) : BackgroundService
{
    private readonly AnomalyDetectionOptions _options = options.Value;
    private readonly ConcurrentDictionary<string, ServerStatistics> _lastStatsByServer;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumer.SubscribeAsync<ServerStatistics>(
            "ServerStatistics.*",
            (stats, routingKey, ct) => HandleStatisticsAsync(stats, ct),
            stoppingToken);
        
        logger.LogInformation("Subscribed to 'ServerStatistics.*'. Waiting for messages...");
        
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    
    private async Task HandleStatisticsAsync(ServerStatistics stats, CancellationToken ct)
    {
        await repository.SaveAsync(stats, ct);

        if (_lastStatsByServer.TryGetValue(stats.ServerIdentifier, out var previous))
            await CheckAnomalyAsync(previous, stats, ct);

        await CheckHighUsageAsync(stats, ct);

        _lastStatsByServer[stats.ServerIdentifier] = stats;
    }
    
    private async Task CheckAnomalyAsync(ServerStatistics previous, ServerStatistics current, CancellationToken ct)
    {
        if (current.MemoryUsage > previous.MemoryUsage * (1 + _options.MemoryUsageAnomalyThresholdPercentage))
        {
            logger.LogWarning("Memory anomaly on {Server}: {Prev}MB -> {Curr}MB",
                current.ServerIdentifier, previous.MemoryUsage, current.MemoryUsage);

            await alertNotifier.SendAlertAsync(new AlertMessage
            {
                AlertType = "AnomalyAlert",
                ServerIdentifier = current.ServerIdentifier,
                Metric = "Memory",
                Message = $"Sudden memory usage increase on '{current.ServerIdentifier}': " +
                          $"{previous.MemoryUsage:F0}MB -> {current.MemoryUsage:F0}MB.",
                Timestamp = current.Timestamp
            }, ct);
        }

        if (current.CpuUsage > previous.CpuUsage * (1 + _options.CpuUsageAnomalyThresholdPercentage))
        {
            logger.LogWarning("CPU anomaly on {Server}: {Prev}% -> {Curr}%",
                current.ServerIdentifier, previous.CpuUsage, current.CpuUsage);

            await alertNotifier.SendAlertAsync(new AlertMessage
            {
                AlertType = "AnomalyAlert",
                ServerIdentifier = current.ServerIdentifier,
                Metric = "Cpu",
                Message = $"Sudden CPU usage increase on '{current.ServerIdentifier}': " +
                          $"{previous.CpuUsage:F1}% -> {current.CpuUsage:F1}%.",
                Timestamp = current.Timestamp
            }, ct);
        }
    }
    private async Task CheckHighUsageAsync(ServerStatistics current, CancellationToken ct)
    {
        double totalMemory = current.MemoryUsage + current.AvailableMemory;
        double memoryUsagePercentage = totalMemory > 0 ? current.MemoryUsage / totalMemory : 0;

        if (memoryUsagePercentage > _options.MemoryUsageThresholdPercentage)
        {
            await alertNotifier.SendAlertAsync(new AlertMessage
            {
                AlertType = "HighUsageAlert",
                ServerIdentifier = current.ServerIdentifier,
                Metric = "Memory",
                Message = $"High memory usage on '{current.ServerIdentifier}': {memoryUsagePercentage:P0}.",
                Timestamp = current.Timestamp
            }, ct);
        }

        double cpuThresholdPercent = _options.CpuUsageThresholdPercentage * 100;
        if (current.CpuUsage > cpuThresholdPercent)
        {
            await alertNotifier.SendAlertAsync(new AlertMessage
            {
                AlertType = "HighUsageAlert",
                ServerIdentifier = current.ServerIdentifier,
                Metric = "Cpu",
                Message = $"High CPU usage on '{current.ServerIdentifier}': {current.CpuUsage:F1}%.",
                Timestamp = current.Timestamp
            }, ct);
        }
    }
}