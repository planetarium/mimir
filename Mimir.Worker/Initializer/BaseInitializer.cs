using Mimir.Worker.Services;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Initializer;

public abstract class BaseInitializer
{
    protected IStateService _stateService;

    protected MongoDbService _store;

    protected readonly ILogger _logger;

    protected BaseInitializer(IStateService stateService, MongoDbService store, ILogger logger)
    {
        _stateService = stateService;
        _store = store;
        _logger = logger;
    }

    public abstract Task RunAsync(CancellationToken stoppingToken);
    public abstract Task<bool> IsInitialized();
}
