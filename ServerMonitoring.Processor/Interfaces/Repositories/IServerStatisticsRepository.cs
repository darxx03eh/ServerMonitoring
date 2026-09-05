using ServerMonitoring.Processor.Models;

namespace ServerMonitoring.Processor.Interfaces.Repositories;

public interface IServerStatisticsRepository
{
    Task SaveAsync(ServerStatistics statistics, CancellationToken cancellationToken = default);
}