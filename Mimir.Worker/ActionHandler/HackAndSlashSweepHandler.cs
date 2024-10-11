using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Services;
using Mimir.Worker.CollectionUpdaters;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class HackAndSlashSweepHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler(
        stateService,
        store,
        "^hack_and_slash_sweep[0-9]*$",
        Log.ForContext<HackAndSlashSweepHandler>())
{
    private static readonly HackAndSlashSweep Action = new();

    protected override async Task HandleAction(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        Action.LoadPlainValue(actionPlainValue);
        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Adventure,
            Action.avatarAddress,
            Action.costumes,
            Action.equipments,
            session,
            stoppingToken);
    }
}
