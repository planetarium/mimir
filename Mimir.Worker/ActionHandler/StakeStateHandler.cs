using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Action;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class StakeStateHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler(stateService, store, "^stake[0-9]*$", Log.ForContext<StakeStateHandler>())
{
    protected override async Task HandleActionAsync(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        var action = new Stake();
        action.LoadPlainValue(actionPlainValue);
        await StakeCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            signer,
            session,
            stoppingToken);
    }
}
