namespace ServerMonitoring.StatsCollector.Options;

public class ServerStatisticsOptions
{
    public int SamplingIntervalSeconds { get; set; } = 60;
    public string ServerIdentifier { get; set; } = Environment.MachineName;
}