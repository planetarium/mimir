using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.Services;
using Mimir.Worker.CollectionUpdaters;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class RaidHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(stateService, store, "^raid[0-9]*$", Log.ForContext<RaidHandler>())
{
    protected override async Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IAction action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        if (action is not IRaidV2 raid)
        {
            return false;
        }

        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Raid,
            raid.AvatarAddress,
            raid.CostumeIds,
            raid.EquipmentIds,
            session,
            stoppingToken
        );

        return true;
    }
}
