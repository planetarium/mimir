using Lib9c.Abstractions;
using Libplanet.Action;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.Handler;

public class RaidHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler(
        stateService,
        store,
        "^raid[0-9]*$",
        Log.ForContext<RaidHandler>())
{
    protected override async Task HandleAction(long blockIndex, IAction action)
    {
        if (action is not IRaidV2 raid)
        {
            throw new NotImplementedException(
                $"Action is not {nameof(IRaidV2)}: {action.GetType()}");
        }

        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Raid,
            raid.AvatarAddress,
            raid.CostumeIds,
            raid.EquipmentIds);
    }
}
