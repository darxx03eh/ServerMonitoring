using System.Diagnostics;
using ServerMonitoring.StatsCollector.IProviders;
using ServerMonitoring.StatsCollector.Models;

namespace ServerMonitoring.StatsCollector.Providers;

public class SystemDiagnosticsStatisticsProvider : IServerStatisticsProvider
{
    private static readonly TimeSpan CpuSampleWindow = TimeSpan.FromMilliseconds(500);
    public ServerStatistics GetCurrentStatistics(string serverIdentifier)
    {
        double cpuUsage = GetCpuUsagePercentage();
        double usedMemoryMb = GetUsedMemoryMb();
        double totalMemoryMb = GetTotalPhysicalMemoryMb();
        double availableMemoryMb = Math.Max(totalMemoryMb - usedMemoryMb, 0);

        return new ServerStatistics
        {
            ServerIdentifier = serverIdentifier,
            CpuUsage = cpuUsage,
            MemoryUsage = usedMemoryMb,
            AvailableMemory = availableMemoryMb,
            Timestamp = DateTime.UtcNow
        };
    }
    
    private static double GetCpuUsagePercentage()
    {
        var startTime = DateTime.UtcNow;
        var startCpuTime = GetTotalProcessorTime();

        Thread.Sleep(CpuSampleWindow);

        var endTime = DateTime.UtcNow;
        var endCpuTime = GetTotalProcessorTime();

        double cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
        double elapsedMs = (endTime - startTime).TotalMilliseconds;

        double usage = cpuUsedMs / (Environment.ProcessorCount * elapsedMs) * 100.0;
        return Math.Clamp(usage, 0, 100);
    }
    private static TimeSpan GetTotalProcessorTime()
    {
        TimeSpan total = TimeSpan.Zero;
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                total += process.TotalProcessorTime;
            }
            catch
            {
                // Process may have exited, or access is denied (e.g. system processes on Windows).
            }
            finally
            {
                process.Dispose();
            }
        }
        return total;
    }
    private static double GetUsedMemoryMb()
    {
        long totalWorkingSetBytes = 0;
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                totalWorkingSetBytes += process.WorkingSet64;
            }
            catch
            {
                // Same as above: some processes may not be accessible.
            }
            finally
            {
                process.Dispose();
            }
        }
        return totalWorkingSetBytes / (1024.0 * 1024.0);
    }

    private static double GetTotalPhysicalMemoryMb()
    {
        var gcInfo = GC.GetGCMemoryInfo();
        return gcInfo.TotalAvailableMemoryBytes / (1024.0 * 1024.0);
    }
}