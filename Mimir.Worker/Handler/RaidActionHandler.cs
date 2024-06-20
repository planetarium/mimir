using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume;
using Nekoyume.Extensions;
using Nekoyume.TableData;
using Serilog;

namespace Mimir.Worker.Handler;

public class RaidActionHandler : BaseActionHandler
{
    public RaidActionHandler(IStateService stateService, MongoDbService store)
        : base(stateService, store, "^raid[0-9]*$", Log.ForContext<RaidActionHandler>()) { }

    public override async Task HandleAction(
        string actionType,
        long processBlockIndex,
        Dictionary actionValues
    )
    {
        var avatarAddress = new Address(actionValues["a"]);

        _logger.Information("Handle raid, avatar: {avatarAddress}", avatarAddress);

        var worldBossListSheet = await _store.GetSheetAsync<WorldBossListSheet>();

        if (worldBossListSheet != null)
        {
            var row = worldBossListSheet.FindRowByBlockIndex(processBlockIndex);
            int raidId = row.Id;
            var worldBossAddress = Addresses.GetWorldBossAddress(raidId);
            var raiderAddress = Addresses.GetRaiderAddress(avatarAddress, raidId);
            var worldBossKillRewardRecordAddress = Addresses.GetWorldBossKillRewardRecordAddress(
                avatarAddress,
                raidId
            );
        }
    }
}
