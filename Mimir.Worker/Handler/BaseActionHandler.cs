using Bencodex.Types;
using Mimir.Worker.Util;
using Mimir.Worker.Services;

namespace Mimir.Worker.Handler;

public abstract class BaseActionHandler
{
    protected IStateService _stateService;

    protected StateGetter _stateGetter;

    protected MongoDbService _store;

    public readonly string ActionRegex;

    protected BaseActionHandler(
        IStateService stateService,
        MongoDbService store,
        string actionRegex
    )
    {
        _stateService = stateService;
        _stateGetter = new StateGetter(stateService);
        _store = store;
        ActionRegex = actionRegex;
    }

    public abstract Task HandleAction(string actionType, long processBlockIndex, Dictionary actionValues);
}
