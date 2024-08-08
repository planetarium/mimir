using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.Services;
using Mimir.Worker.CollectionUpdaters;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class HackAndSlashSweepHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^hack_and_slash_sweep[0-9]*$",
        Log.ForContext<HackAndSlashSweepHandler>()
    )
{
    protected override async Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IAction action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        if (action is not IHackAndSlashSweepV3 hackAndSlashSweep)
        {
            return false;
        }

        Logger.Information(
            "Handle hack_and_slash_sweep, address: {AvatarAddress}",
            hackAndSlashSweep.AvatarAddress
        );

        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Adventure,
            hackAndSlashSweep.AvatarAddress,
            hackAndSlashSweep.Costumes,
            hackAndSlashSweep.Equipments,
            session,
            stoppingToken
        );

        return true;
    }
}
