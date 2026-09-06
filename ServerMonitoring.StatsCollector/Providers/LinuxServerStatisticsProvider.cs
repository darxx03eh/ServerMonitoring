using ServerMonitoring.StatsCollector.IProviders;
using ServerMonitoring.StatsCollector.Models;

namespace ServerMonitoring.StatsCollector.Providers;

public class LinuxServerStatisticsProvider : IServerStatisticsProvider
{
    
    public ServerStatistics GetCurrentStatistics(string serverIdentifier)
    {
        double cpuUsage = GetCpuUsagePercentage();
        var (usedMb, availableMb) = GetMemoryMb();

        return new()
        {
            ServerIdentifier = serverIdentifier,
            CpuUsage = cpuUsage,
            MemoryUsage = usedMb,
            AvailableMemory = availableMb,
            Timestamp = DateTime.UtcNow
        };
    }

    private static double GetCpuUsagePercentage()
    {
        var (idle1, total1) = ReadCpuTimes();
        Thread.Sleep(IServerStatisticsProvider.CpuSampleWindow);
        var (idle2, total2) = ReadCpuTimes();

        long totalDelta = total2 - total1;
        if (totalDelta <= 0) return 0;

        double usage = (1.0 - (double)(idle2 - idle1) / totalDelta) * 100.0;
        return Math.Clamp(usage, 0, 100);
    }

    private static (double usedMb, double availableMb) GetMemoryMb()
    {
        var meminfo = File.ReadLines("/proc/meminfo")
            .Select(l => l.Split(':', 2))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0].Trim(), p => ParseKb(p[1]));

        double totalKb = meminfo.GetValueOrDefault("MemTotal", 0);
        double availableKb = meminfo.GetValueOrDefault("MemAvailable", 0);
        double usedKb = Math.Max(totalKb - availableKb, 0);
        
        return  (usedKb / 1024.0, availableKb / 1024.0);
    }

    private static (long idle, long total) ReadCpuTimes()
    {
        string line = File.ReadLines("/proc/stat").First();
        long[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(long.Parse)
            .ToArray();

        long idle = values[3] + values[4];
        long total = values.Sum();
        return (idle, total);
    }

    private static long ParseKb(string value)
    {
        var digits = value.Trim().Split(' ')[0];
        return long.TryParse(digits, out var kb) ? kb : 0;
    }
}