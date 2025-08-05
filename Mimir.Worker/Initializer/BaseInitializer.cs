using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Initializer;

public abstract class BaseInitializer : BackgroundService
{
    protected IStateService _stateService;

    protected IMongoDbService _store;

    protected readonly ILogger _logger;

    protected BaseInitializer(IStateService stateService, IMongoDbService store, ILogger logger)
    {
        logger.Information("LOG");
        _stateService = stateService;
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await IsInitialized())
        {
            await RunAsync(stoppingToken);   
        }
    }

    public abstract Task RunAsync(CancellationToken stoppingToken);
    public abstract Task<bool> IsInitialized();
}
