using Mimir.Worker.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Initializer;

public class InitializerManager
{
    private readonly List<BaseInitializer> _initializers;

    protected readonly ILogger _logger;

    public InitializerManager(IStateService stateService, MongoDbService store)
    {
        _initializers = new List<BaseInitializer>
        {
            new TableSheetInitializer(stateService, store),
            // new MarketInitializer(stateService, store)
        };
        _logger = Log.ForContext<InitializerManager>();
    }

    public async Task RunInitializersAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();

        foreach (var initializer in _initializers)
        {
            if (!await initializer.IsInitialized())
            {
                tasks.Add(initializer.RunAsync(stoppingToken));
            }
        }

        _logger.Information("Start {Count} initializing", tasks.Count());
        await Task.WhenAll(tasks);
    }
}
