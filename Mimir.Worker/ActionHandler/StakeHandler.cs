using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class StakeHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(stateService, store, "^stake[0-9]*$", Log.ForContext<StakeHandler>())
{
    protected override async Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IAction action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        if (action is not IStakeV1)
        {
            return false;
        }

        await StakeCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            signer,
            session,
            stoppingToken
        );

        return true;
    }
}
