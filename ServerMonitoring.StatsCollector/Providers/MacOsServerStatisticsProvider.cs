using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using ServerMonitoring.StatsCollector.IProviders;
using ServerMonitoring.StatsCollector.Models;

namespace ServerMonitoring.StatsCollector.Providers;

[SupportedOSPlatform("macOS")]
public class MacOsServerStatisticsProvider : IServerStatisticsProvider
{
    public ServerStatistics GetCurrentStatistics(string serverIdentifier)
    {
        double cpuUsage = GetCpuUsagePercentage();
        var (usedMb, availableMb) = GetMemoryMb();

        return new ServerStatistics
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
        string output = RunCommand("top", "-l 2 -n 0");
        var matches = Regex.Matches(output,
            @"CPU usage:\s*([\d.]+)%\s*user,\s*([\d.]+)%\s*sys,\s*([\d.]+)%\s*idle");

        if (matches.Count == 0) return 0;

        var last = matches[^1];
        double user = double.Parse(last.Groups[1].Value);
        double sys = double.Parse(last.Groups[2].Value);
        return Math.Clamp(user + sys, 0, 100);
    }

    private static (double usedMb, double availableMb) GetMemoryMb()
    {
        string vmStat = RunCommand("vm_stat", string.Empty);

        long pageSize = 4096;
        var pageSizeMatch = Regex.Match(vmStat, @"page size of (\d+) bytes");
        if (pageSizeMatch.Success)
            pageSize = long.Parse(pageSizeMatch.Groups[1].Value);

        long free = ExtractPages(vmStat, "Pages free");
        long inactive = ExtractPages(vmStat, "Pages inactive");
        long active = ExtractPages(vmStat, "Pages active");
        long wired = ExtractPages(vmStat, "Pages wired down");
        long compressed = ExtractPages(vmStat, "Pages occupied by compressor");

        double availableMb = (free + inactive) * pageSize / (1024.0 * 1024.0);
        double usedMb = (active + wired + compressed) * pageSize / (1024.0 * 1024.0);

        return (usedMb, availableMb);
    }

    private static long ExtractPages(string vmStat, string label)
    {
        var match = Regex.Match(vmStat, $@"{label}:\s*(\d+)\.");
        return match.Success ? long.Parse(match.Groups[1].Value) : 0;
    }
    private static string RunCommand(string fileName, string arguments)
    {
        using var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }
}