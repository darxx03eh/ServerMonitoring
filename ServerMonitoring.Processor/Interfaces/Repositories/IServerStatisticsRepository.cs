using ServerMonitoring.Processor.Models;

namespace ServerMonitoring.Processor.Interfaces.Repositories;

public interface IServerStatisticsRepository
{
    Task SaveAsync(ServerStatistics serverStatistics, CancellationToken cancellationToken = default);
}