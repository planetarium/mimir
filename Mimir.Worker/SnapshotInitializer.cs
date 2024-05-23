using HeadlessGQL;
using Libplanet.RocksDBStore;
using Libplanet.Store;
using Mimir.Worker.Scrapper;
using Mimir.Worker.Services;

namespace Mimir.Worker;

public class SnapshotInitializer
{
    private readonly DiffMongoDbService _store;
    private readonly ILogger<SnapshotInitializer> _logger;
    private readonly IStateStore _chainStateStore;

    public SnapshotInitializer(
        ILogger<SnapshotInitializer> logger,
        DiffMongoDbService store,
        string chainStorePath
    )
    {
        _logger = logger;
        _store = store;

        Uri uri = new Uri(chainStorePath);
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;

        _logger.LogInformation(
            "Finished SnapshotInitializer. Elapsed {TotalElapsedMinutes} minutes",
            DateTime.UtcNow.Subtract(started).Minutes
        );
    }
}
