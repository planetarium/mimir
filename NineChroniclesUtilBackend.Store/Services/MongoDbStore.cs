using MongoDB.Driver;

namespace NineChroniclesUtilBackend.Store.Services;

public class MongoDbStore(ILogger<MongoDbStore> logger, string connectionString, string databaseName)
{
    private readonly ILogger<MongoDbStore> _logger = logger;
    private readonly IMongoClient _client = new MongoClient(connectionString);

    public async Task WithTransaction(Func<IStateStorage, CancellationToken, Task> action)
    {
        using var session = await _client.StartSessionAsync();
        await session.WithTransaction((s, ct) =>
        {
            var database = s.Client.GetDatabase(databaseName);
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<StateStorage>();
            var storage = new StateStorage(logger, database);
            return action(storage, ct);
        });
    }

    public async Task<bool> IsInitialized()
    {
        var names = await (await _client.GetDatabase(databaseName).ListCollectionNamesAsync()).ToListAsync();
        return names is not { } ns || !(ns.Contains("arena") && ns.Contains("avatars"));
    }
}
