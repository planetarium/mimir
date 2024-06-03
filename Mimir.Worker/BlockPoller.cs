using Bencodex;
using Bencodex.Types;
using HeadlessGQL;
using Libplanet.Common;
using Libplanet.Crypto;
using Libplanet.Types.Tx;
using Mimir.Worker.Models;
using Mimir.Worker.Scrapper;
using Mimir.Worker.Services;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using StrawberryShake;

namespace Mimir.Worker;

public class BlockPoller(
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    MongoDbStore mongoDbStore
)
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

            await EveryAvatarAsync(processBlockIndex, stateGetter, cancellationToken);
            await BattleArenaAsync(processBlockIndex, stateGetter, cancellationToken);
            await EveryPatchTableAsync(processBlockIndex, cancellationToken);
            await mongoDbStore.UpdateLatestBlockIndex(processBlockIndex);
        }
    }

    private async Task EveryAvatarAsync(
        long processBlockIndex,
        StateGetter stateGetter,
        CancellationToken cancellationToken
    )
    {
        var operationResult = await headlessGqlClient.GetTransactionSigners.ExecuteAsync(
            processBlockIndex,
            1,
            cancellationToken
        );
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
            var avatarAddresses = Enumerable
                .Range(0, GameConfig.SlotCount)
                .Select(e => Addresses.GetAvatarAddress(agentAddress, e));
            var avatarDataArray = await Task.WhenAll(
                avatarAddresses.Select(stateGetter.GetAvatarData)
            );
            await mongoDbStore.BulkUpsertAvatarDataAsync(
                avatarDataArray.Where(e => e is not null).OfType<AvatarData>().ToList()
            );
        }
    }

    private async Task EveryPatchTableAsync(
        long processBlockIndex,
        CancellationToken cancellationToken
    )
    {
        var rawPatchTableTxsResp = await headlessGqlClient.GetPatchTableTransactions.ExecuteAsync(
            processBlockIndex,
            cancellationToken
        );
        if (rawPatchTableTxsResp.Data is null)
        {
            HandleErrors(rawPatchTableTxsResp);
            return;
        }
        var sheetTypes = typeof(ISheet)
            .Assembly.GetTypes()
            .Where(type =>
                type.Namespace is { } @namespace
                && @namespace.StartsWith($"{nameof(Nekoyume)}.{nameof(Nekoyume.TableData)}")
                && !type.IsAbstract
                && typeof(ISheet).IsAssignableFrom(type)
            );

        var patchTableTxs = rawPatchTableTxsResp
            .Data?.Transaction?.NcTransactions.Where(raw => raw is not null)
            .Select(raw =>
                TxMarshaler.DeserializeTransactionWithoutVerification(
                    Convert.FromBase64String(raw!.SerializedPayload)
                )
            )
            .ToList();
        foreach (var patchTableTx in patchTableTxs)
        {
            var patchTableAction = (Dictionary)patchTableTx.Actions[0];
            var patchTableActionValues = (Dictionary)patchTableAction["values"];
            var tableName = ((Text)patchTableActionValues["table_name"]).ToDotnetString();

            var sheetType = sheetTypes.Where(type => type.Name == tableName).FirstOrDefault();

            if (sheetType == null)
            {
                throw new TypeLoadException(
                    $"Unable to find a class type matching the table name '{tableName}' in the specified namespace."
                );
            }
            var sheetInstance = Activator.CreateInstance(sheetType);
            if (sheetInstance is not ISheet sheet)
            {
                throw new InvalidCastException($"Type {sheetType.Name} cannot be cast to ISheet.");
            }
            var sheetAddress = Addresses.TableSheet.Derive(tableName);
            var sheetState = await stateService.GetState(sheetAddress);
            if (sheetState is not Text sheetValue)
            {
                throw new InvalidOperationException($"Expected sheet state to be of type 'Text'.");
            }

            sheet.Set(sheetValue.Value);

            var sheetData = new TableSheetData(
                sheetAddress,
                tableName,
                sheet,
                sheetState.ToDotnetString(),
                ByteUtil.Hex(new Codec().Encode(sheetState))
            );

            await mongoDbStore.InsertTableSheets(sheetData);
        }
    }

    private async Task BattleArenaAsync(
        long processBlockIndex,
        StateGetter stateGetter,
        CancellationToken cancellationToken
    )
    {
        var rawArenaTxsResp = await headlessGqlClient.GetBattleArenaTransactions.ExecuteAsync(
            processBlockIndex,
            cancellationToken
        );
        if (rawArenaTxsResp.Data is null)
        {
            HandleErrors(rawArenaTxsResp);
            return;
        }

        if (rawArenaTxsResp.Data.Transaction.NcTransactions is null)
        {
            return;
        }

        var arenaTxs = rawArenaTxsResp
            .Data.Transaction.NcTransactions.Where(raw => raw is not null)
            .Select(raw =>
                TxMarshaler.DeserializeTransactionWithoutVerification(
                    Convert.FromBase64String(raw!.SerializedPayload)
                )
            )
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

            await mongoDbStore.BulkUpsertAvatarDataAsync(
                new[] { myAvatarData, enemyAvatarData }
                    .Where(e => e is not null)
                    .OfType<AvatarData>()
                    .ToList()
            );
            await mongoDbStore.BulkUpsertArenaDataAsync(
                new[] { myArenaData, enemyArenaData }
                    .Where(e => e is not null)
                    .OfType<ArenaData>()
                    .ToList()
            );
        }
    }

    private static void HandleErrors(IOperationResult operationResult)
    {
        var errors = operationResult.Errors.Select(e => e.Message);
        Serilog.Log.Error("Failed to get txs. response data is null. errors:\n{Errors}", errors);
    }
}
