namespace ServerMonitoring.Processor.Models;

public class ServerStatistics
{
    public string ServerIdentifier { get; set; } = string.Empty;
    public double MemoryUsage { get; set; }
    public double AvailableMemory { get; set; }
    public double CpuUsage { get; set; }
    public DateTime Timestamp { get; set; }
}