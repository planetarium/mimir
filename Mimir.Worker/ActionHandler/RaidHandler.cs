using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Constants;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume;
using Nekoyume.Extensions;
using Nekoyume.Model.EnumType;
using Nekoyume.TableData;
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

        var avatarAddress = raid.AvatarAddress;

        Logger.Information("Handle raid, avatar: {avatarAddress}", avatarAddress);

        var worldBossListSheet = await Store.GetSheetAsync<WorldBossListSheet>();

        if (worldBossListSheet != null)
        {
            int raidId;
            try
            {
                var row = worldBossListSheet.FindRowByBlockIndex(blockIndex);
                raidId = row.Id;
            }
            catch (InvalidOperationException)
            {
                Logger.Error("Failed to get this raidId.");
                return false;
            }

            var worldBossAddress = Addresses.GetWorldBossAddress(raidId);
            var raiderAddress = Addresses.GetRaiderAddress(avatarAddress, raidId);
            var worldBossKillRewardRecordAddress = Addresses.GetWorldBossKillRewardRecordAddress(
                avatarAddress,
                raidId
            );

            var worldBossState = await StateGetter.GetWorldBossState(worldBossAddress);
            var raiderState = await StateGetter.GetRaiderState(raiderAddress);
            var worldBossKillRewardRecordState =
                await StateGetter.GetWorldBossKillRewardRecordState(
                    worldBossKillRewardRecordAddress
                );

            await Store.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<WorldBossStateDocument>(),
                [new WorldBossStateDocument(worldBossAddress, raidId, worldBossState)],
                session,
                stoppingToken
            );
            await Store.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<RaiderStateDocument>(),
                [new RaiderStateDocument(raiderAddress, raiderState)],
                session,
                stoppingToken
            );
            await Store.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<WorldBossKillRewardRecordDocument>(),
                [
                    new WorldBossKillRewardRecordDocument(
                        worldBossKillRewardRecordAddress,
                        avatarAddress,
                        worldBossKillRewardRecordState
                    )
                ],
                session,
                stoppingToken
            );
        }
        else
        {
            Logger.Error("RaidActionHandler requires worldBossListSheet.");
        }

        return true;
    }
}
