using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume;
using Nekoyume.Extensions;
using Nekoyume.TableData;
using Serilog;

namespace Mimir.Worker.Handler;

public class RaidActionHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(stateService, store, "^raid[0-9]*$", Log.ForContext<RaidActionHandler>())
{
    protected override async Task HandleAction(
        string actionType,
        long processBlockIndex,
        Dictionary actionValues
    )
    {
        var avatarAddress = new Address(actionValues["a"]);

        Logger.Information("Handle raid, avatar: {avatarAddress}", avatarAddress);

        var worldBossListSheet = await Store.GetSheetAsync<WorldBossListSheet>();

        if (worldBossListSheet != null)
        {
            int raidId;
            try
            {
                var row = worldBossListSheet.FindRowByBlockIndex(processBlockIndex);
                raidId = row.Id;
            }
            catch (InvalidOperationException)
            {
                Logger.Error("Failed to get this raidId.");
                return;
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

            await Store.UpsertStateDataAsync(
                new StateData(
                    worldBossAddress,
                    new WorldBossState(worldBossAddress, raidId, worldBossState)
                )
            );
            await Store.UpsertStateDataAsync(
                new StateData(raiderAddress, new RaiderState(raiderAddress, raiderState))
            );
            await Store.UpsertStateDataAsync(
                new StateData(
                    worldBossKillRewardRecordAddress,
                    new WorldBossKillRewardRecordState(
                        worldBossKillRewardRecordAddress,
                        avatarAddress,
                        worldBossKillRewardRecordState
                    )
                )
            );
        }
        else
        {
            Logger.Error("RaidActionHandler requires worldBossListSheet.");
        }
    }
}
