using ServerMonitoring.StatsCollector.Providers;

namespace ServerMonitoring.StatsCollector.IProviders.Factory;

public static class ServerStatisticsProviderFactory
{
    public static IServerStatisticsProvider Create()
    {
        if (OperatingSystem.IsLinux())
            return new LinuxServerStatisticsProvider();
        if (OperatingSystem.IsWindows())
            return new WindowsServerStatisticsProvider();
        if (OperatingSystem.IsMacOS())
            return new MacOsServerStatisticsProvider();
        
        throw new PlatformNotSupportedException(
            $"No {nameof(IServerStatisticsProvider)} implementation available for the current OS.");
    }
}