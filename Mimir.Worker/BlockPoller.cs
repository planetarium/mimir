using Bencodex.Types;
using HeadlessGQL;
using Libplanet.Crypto;
using Libplanet.Types.Tx;
using Mimir.Worker.Models;
using Mimir.Worker.Scrapper;
using Mimir.Worker.Services;

namespace Mimir.Worker;

public class BlockPoller(IStateService stateService, HeadlessGQLClient headlessGqlClient, MongoDbStore mongoDbStore)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var stateGetter = new StateGetter(stateService);
        while (!cancellationToken.IsCancellationRequested)
        {
            var syncedBlockIndex = await mongoDbStore.GetLatestBlockIndex();
            var currentBlockIndex = await stateService.GetLatestIndex();
            var processBlockIndex = syncedBlockIndex + 1;

            if (processBlockIndex >= currentBlockIndex)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(3000), cancellationToken);
                continue;
            }

            var rawArenaTxsResp = await headlessGqlClient.GetBattleArenaTransactions.ExecuteAsync(processBlockIndex);

            if (rawArenaTxsResp.Data is null)
            {
                Serilog.Log.Error("Failed to get arena txs. errors:\n" +
                                  string.Join("\n", rawArenaTxsResp.Errors.Select(x => "- " + x.Message)));
                await mongoDbStore.UpdateLatestBlockIndex(syncedBlockIndex + 1);
                continue;
            }

            try
            {
                var arenaTxs = rawArenaTxsResp.Data.Transaction
                    .NcTransactions!.Select(raw =>
                        TxMarshaler.DeserializeTransactionWithoutVerification(
                            Convert.FromBase64String(raw.SerializedPayload)))
                    .ToList();

                foreach (var arenaTx in arenaTxs)
                {
                    var arenaAction = (Dictionary)arenaTx.Actions.First();
                    var arenaActionValues = (Dictionary)arenaAction["values"];
                    var enemyAvatarAddress = new Address(arenaActionValues["eaa"]);
                    var myAvatarAddress = new Address(arenaActionValues["maa"]);

                    var roundData = await stateGetter.GetArenaRoundData(processBlockIndex);
                    var enemyAvatarData = await stateGetter.GetAvatarData(enemyAvatarAddress);
                    var myAvatarData = await stateGetter.GetAvatarData(myAvatarAddress);
                    var myArenaData = await stateGetter.GetArenaData(roundData, myAvatarAddress);
                    var enemyArenaData = await stateGetter.GetArenaData(roundData, enemyAvatarAddress);

                    await mongoDbStore.BulkUpsertAvatarDataAsync(new List<AvatarData> { myAvatarData, enemyAvatarData });
                    await mongoDbStore.BulkUpsertArenaDataAsync(new List<ArenaData> { myArenaData, enemyArenaData });
                }
            }
            finally
            {
                await mongoDbStore.UpdateLatestBlockIndex(syncedBlockIndex + 1);
            }
        }
    }
}
