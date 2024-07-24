using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class HackAndSlashHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^hack_and_slash[0-9]*$",
        Log.ForContext<HackAndSlashHandler>()
    )
{
    protected override async Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IAction action,
        IClientSessionHandle? session = null
    )
    {
        if (action is not IHackAndSlashV10 hackAndSlash)
        {
            return false;
        }

        Logger.Information(
            "Handle hack_and_slash, address: {AvatarAddress}",
            hackAndSlash.AvatarAddress
        );

        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Adventure,
            hackAndSlash.AvatarAddress,
            hackAndSlash.Costumes,
            hackAndSlash.Equipments,
            session
        );

        return true;
    }
}
