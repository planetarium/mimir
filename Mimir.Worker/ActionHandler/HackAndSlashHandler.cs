using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Services;
using Mimir.Worker.CollectionUpdaters;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class HackAndSlashHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler(
        stateService,
        store,
        "^hack_and_slash[0-9]*$",
        Log.ForContext<HackAndSlashHandler>())
{
    protected override async Task HandleAction(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        var action = new HackAndSlash();
        action.LoadPlainValue(actionPlainValue);
        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Adventure,
            action.AvatarAddress,
            action.Costumes,
            action.Equipments,
            session,
            stoppingToken);
    }
}
