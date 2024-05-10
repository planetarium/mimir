using Bencodex.Types;
using HeadlessGQL;
using Libplanet.Crypto;
using Libplanet.Types.Tx;
using Mimir.Worker.Models;
using Mimir.Worker.Scrapper;
using Mimir.Worker.Services;
using Nekoyume;
using StrawberryShake;

namespace Mimir.Worker;

public class BlockPoller(IStateService stateService, IHeadlessGQLClient headlessGqlClient, MongoDbWorker mongoDbWorker)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var stateGetter = new StateGetter(stateService);
        while (!cancellationToken.IsCancellationRequested)
        {
            var syncedBlockIndex = await mongoDbWorker.GetLatestBlockIndex();
            var currentBlockIndex = await stateService.GetLatestIndex();
            var processBlockIndex = syncedBlockIndex + 1;
            if (processBlockIndex >= currentBlockIndex)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(3000), cancellationToken);
                continue;
            }

            await EveryAvatarAsync(processBlockIndex, stateGetter, cancellationToken);
            await BattleArenaAsync(processBlockIndex, stateGetter, cancellationToken);
            await mongoDbWorker.UpdateLatestBlockIndex(processBlockIndex);
        }
    }

    private async Task EveryAvatarAsync(
        long processBlockIndex,
        StateGetter stateGetter,
        CancellationToken cancellationToken)
    {
        var operationResult = await headlessGqlClient.GetTransactionSigners.ExecuteAsync(
            processBlockIndex,
            cancellationToken);
        if (operationResult.Data is null)
        {
            HandleErrors(operationResult);
            return;
        }

        var txs = operationResult.Data.Transaction.NcTransactions;
        if (txs is null || txs.Count == 0)
        {
            return;
        }

        foreach (var tx in txs)
        {
            if (tx is null)
            {
                continue;
            }

            var agentAddress = new Address(tx.Signer);
            var avatarAddresses = Enumerable.Range(0, GameConfig.SlotCount)
                .Select(e => Addresses.GetAvatarAddress(agentAddress, e));
            var avatarDataArray = await Task.WhenAll(avatarAddresses.Select(stateGetter.GetAvatarData));
            await mongoDbWorker.BulkUpsertAvatarDataAsync(
                avatarDataArray
                    .Where(e => e is not null)
                    .OfType<AvatarData>()
                    .ToList());
        }
    }

    private async Task BattleArenaAsync(
        long processBlockIndex,
        StateGetter stateGetter,
        CancellationToken cancellationToken)
    {
        var rawArenaTxsResp = await headlessGqlClient.GetBattleArenaTransactions.ExecuteAsync(
            processBlockIndex,
            cancellationToken);
        if (rawArenaTxsResp.Data is null)
        {
            HandleErrors(rawArenaTxsResp);
            return;
        }

        if (rawArenaTxsResp.Data.Transaction.NcTransactions is null)
        {
            return;
        }

        var arenaTxs = rawArenaTxsResp.Data.Transaction.NcTransactions
            .Where(raw => raw is not null)
            .Select(raw => TxMarshaler.DeserializeTransactionWithoutVerification(
                Convert.FromBase64String(raw!.SerializedPayload)))
            .ToList();
        foreach (var arenaTx in arenaTxs)
        {
            var arenaAction = (Dictionary)arenaTx.Actions[0];
            var arenaActionValues = (Dictionary)arenaAction["values"];
            var myAvatarAddress = new Address(arenaActionValues["maa"]);
            var enemyAvatarAddress = new Address(arenaActionValues["eaa"]);

            var roundData = await stateGetter.GetArenaRoundData(processBlockIndex);
            var myAvatarData = await stateGetter.GetAvatarData(myAvatarAddress);
            var enemyAvatarData = await stateGetter.GetAvatarData(enemyAvatarAddress);
            var myArenaData = await stateGetter.GetArenaData(roundData, myAvatarAddress);
            var enemyArenaData = await stateGetter.GetArenaData(roundData, enemyAvatarAddress);

            await mongoDbWorker.BulkUpsertAvatarDataAsync(
                new[] { myAvatarData, enemyAvatarData }
                    .Where(e => e is not null)
                    .OfType<AvatarData>()
                    .ToList());
            await mongoDbWorker.BulkUpsertArenaDataAsync(
                new[] { myArenaData, enemyArenaData }
                    .Where(e => e is not null)
                    .OfType<ArenaData>()
                    .ToList());
        }
    }

    private static void HandleErrors(IOperationResult operationResult)
    {
        var errors = operationResult.Errors.Select(e => e.Message);
        Serilog.Log.Error("Failed to get txs. response data is null. errors:\n{Errors}", errors);
    }
}
