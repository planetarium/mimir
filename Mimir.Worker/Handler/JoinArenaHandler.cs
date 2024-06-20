using Lib9c.Abstractions;
using Libplanet.Action;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.Handler;

public class JoinArenaHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler(
        stateService,
        store,
        "^join_arena[0-9]*$",
        Log.ForContext<JoinArenaHandler>())
{
    protected override async Task HandleAction(long blockIndex, IAction action)
    {
        if (action is not IJoinArenaV1 joinArena)
        {
            throw new NotImplementedException(
                $"Action is not {nameof(IJoinArenaV1)}: {action.GetType()}");
        }

        await ItemSlotCollectionUpdater.UpdateAsync(
            Store,
            BattleType.Arena,
            joinArena.AvatarAddress,
            joinArena.Costumes,
            joinArena.Equipments);
    }
}
