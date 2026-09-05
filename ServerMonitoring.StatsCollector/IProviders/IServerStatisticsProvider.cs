using ServerMonitoring.StatsCollector.Models;

namespace ServerMonitoring.StatsCollector.IProviders;

public interface IServerStatisticsProvider
{
    ServerStatistics GetCurrentStatistics(string serverIdentifier);
}