using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.Handler;

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
        IAction action)
    {
        if (action is not IHackAndSlashV10 hackAndSlash)
        {
            throw new NotImplementedException(
                $"Action is not {nameof(IHackAndSlashV10)}: {action.GetType()}");
        }

        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Adventure,
            hackAndSlash.AvatarAddress,
            hackAndSlash.Costumes,
            hackAndSlash.Equipments);
    }
}
