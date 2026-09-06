using System.Diagnostics;
using System.Runtime.Versioning;
using ServerMonitoring.StatsCollector.IProviders;
using ServerMonitoring.StatsCollector.Models;

namespace ServerMonitoring.StatsCollector.Providers;

[SupportedOSPlatform("windows")]
public class WindowsServerStatisticsProvider : IServerStatisticsProvider, IDisposable
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _availableMemoryCounter;

    public WindowsServerStatisticsProvider()
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _availableMemoryCounter = new PerformanceCounter("Memory", "Available MBytes");

        _cpuCounter.NextValue();
    }
    public ServerStatistics GetCurrentStatistics(string serverIdentifier)
    {
        Thread.Sleep(IServerStatisticsProvider.CpuSampleWindow);
        
        double cpuUsage = _cpuCounter.NextValue();
        double availableMb = _availableMemoryCounter.NextValue();
        double totalMb = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024.0 * 1024.0);
        double usedMb = Math.Max(totalMb - availableMb, 0);
        return new()
        {
            ServerIdentifier = serverIdentifier,
            CpuUsage = cpuUsage,
            AvailableMemory = availableMb,
            Timestamp = DateTime.UtcNow
        };
    }
    public void Dispose()
    {
        _cpuCounter.Dispose();
        _availableMemoryCounter.Dispose();
    }
}