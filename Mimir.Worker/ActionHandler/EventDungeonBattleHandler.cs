using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Services;
using Mimir.Worker.CollectionUpdaters;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class EventDungeonBattleHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler(
        stateService,
        store,
        "^event_dungeon_battle[0-9]*$",
        Log.ForContext<EventDungeonBattleHandler>())
{
    private static readonly EventDungeonBattle Action = new();

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
            Action.AvatarAddress,
            Action.Costumes,
            Action.Equipments,
            session,
            stoppingToken);
    }
}
