using Bencodex.Types;
using Mimir.Worker.Util;
using Mimir.Worker.Services;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Handler;

public abstract class BaseActionHandler
{
    protected IStateService _stateService;

    protected StateGetter _stateGetter;

    protected MongoDbService _store;

    protected readonly ILogger _logger;

    public readonly string ActionRegex;

    protected BaseActionHandler(
        IStateService stateService,
        MongoDbService store,
        string actionRegex,
        ILogger logger
    )
    {
        _stateService = stateService;
        _stateGetter = new StateGetter(stateService);
        _store = store;
        ActionRegex = actionRegex;
        _logger = logger;
    }

    public abstract Task HandleAction(string actionType, long processBlockIndex, Dictionary actionValues);
}
