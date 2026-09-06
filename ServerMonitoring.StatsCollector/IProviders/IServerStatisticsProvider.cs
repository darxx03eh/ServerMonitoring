using ServerMonitoring.StatsCollector.Models;

namespace ServerMonitoring.StatsCollector.IProviders;

public interface IServerStatisticsProvider
{
    public static readonly TimeSpan CpuSampleWindow = TimeSpan.FromMilliseconds(500);
    ServerStatistics GetCurrentStatistics(string serverIdentifier);
}