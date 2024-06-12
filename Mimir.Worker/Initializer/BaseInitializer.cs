using Mimir.Worker.Services;

namespace Mimir.Worker.Initializer;

public abstract class BaseInitializer
{
    protected IStateService _stateService;

    protected MongoDbService _store;

    protected BaseInitializer(IStateService stateService, MongoDbService store)
    {
        _stateService = stateService;
        _store = store;
    }

    public abstract Task RunAsync(CancellationToken stoppingToken);
    public abstract Task<bool> IsInitialized();
}
