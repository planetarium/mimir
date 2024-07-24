using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class EventDungeonBattleHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^event_dungeon_battle[0-9]*$",
        Log.ForContext<EventDungeonBattleHandler>()
    )
{
    protected override async Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IAction action,
        IClientSessionHandle? session = null
    )
    {
        if (action is not IEventDungeonBattleV2 eventDungeonBattle)
        {
            return false;
        }

        Logger.Information(
            "Handle event_dungeon_battle, address: {AvatarAddress}",
            eventDungeonBattle.AvatarAddress
        );

        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Adventure,
            eventDungeonBattle.AvatarAddress,
            eventDungeonBattle.Costumes,
            eventDungeonBattle.Equipments,
            session
        );

        return true;
    }
}
