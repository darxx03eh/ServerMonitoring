using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ServerMonitoring.Processor.Interfaces.Repositories;
using ServerMonitoring.Processor.Models;
using ServerMonitoring.Processor.Options;

namespace ServerMonitoring.Processor.Repositories;

public class MongoServerStatisticsRepository : IServerStatisticsRepository
{
    private readonly IMongoCollection<ServerStatistics> _collection;
    public MongoServerStatisticsRepository(IOptions<MongoOptions> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<ServerStatistics>(settings.CollectionName);
    }

    public async Task SaveAsync(ServerStatistics statistics, CancellationToken cancellationToken = default)
        => await _collection.InsertOneAsync(statistics, cancellationToken: cancellationToken);
}