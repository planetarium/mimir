using Bencodex;
using HeadlessGQL;
using Libplanet.Crypto;
using Libplanet.Types.Tx;
using Mimir.Worker.Constants;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume.Model.State;

namespace Mimir.Worker.Scrapper;

public class DiffScrapper
{
    private readonly HeadlessGQLClient _headlessGqlClient;
    private readonly DiffMongoDbService _store;

    private readonly Codec Codec = new();

    public DiffScrapper(HeadlessGQLClient headlessGqlClient, DiffMongoDbService store)
    {
        _headlessGqlClient = headlessGqlClient;
        _store = store;
    }

    public async Task ExecuteAsync(long baseIndex, long targetIndex)
    {
        long indexDifference = Math.Abs(targetIndex - baseIndex);

        long currentBaseIndex = baseIndex;

        while (indexDifference > 0)
        {
            long currentTargetIndex =
                currentBaseIndex + (indexDifference > 9 ? 9 : indexDifference);

            var diffResult = await _headlessGqlClient.GetDiffs.ExecuteAsync(
                currentBaseIndex,
                currentTargetIndex
            );
            var txs = await GetTransactions(
                currentBaseIndex,
                currentTargetIndex - currentBaseIndex
            );

            if (diffResult.Data?.Diffs != null)
            {
                foreach (var diff in diffResult.Data.Diffs)
                {
                    switch (diff)
                    {
                        case IGetDiffs_Diffs_RootStateDiff rootDiff:
                            ProcessRootStateDiff(rootDiff, txs);
                            break;

                        case IGetDiffs_Diffs_StateDiff stateDiff:
                            ProcessStateDiff(stateDiff, txs);
                            break;
                    }
                }
            }

            currentBaseIndex = currentTargetIndex;
            indexDifference -= 9;
        }
    }

    private async Task<
        IEnumerable<IGetTransactionSigners_Transaction_NcTransactions>
    > GetTransactions(long processBlockIndex, long limit)
    {
        var operationResult = await _headlessGqlClient.GetTransactionSigners.ExecuteAsync(
            processBlockIndex,
            limit
        );

        if (
            operationResult.Data?.Transaction?.NcTransactions == null
            || !operationResult.Data.Transaction.NcTransactions.Any()
        )
        {
            Serilog.Log.Error(
                "No transactions found or null data. Process Block Index: {ProcessBlockIndex}",
                processBlockIndex
            );
            return Enumerable.Empty<IGetTransactionSigners_Transaction_NcTransactions>();
        }

        var txs = operationResult
            .Data.Transaction.NcTransactions.OfType<IGetTransactionSigners_Transaction_NcTransactions>()
            .ToList();

        var txResults = await _headlessGqlClient.GetTransactionResults.ExecuteAsync(
            txs.Select(tx => tx.Id).ToList()
        );

        if (
            txResults.Data?.Transaction?.TransactionResults == null
            || !txResults.Data.Transaction.TransactionResults.Any()
            || txResults.Data.Transaction.TransactionResults.Count != txs.Count
        )
        {
            Serilog.Log.Error(
                "Failed fetch txResults. Process Block Index: {ProcessBlockIndex}",
                processBlockIndex
            );
            return Enumerable.Empty<IGetTransactionSigners_Transaction_NcTransactions>();
        }

        var successfulTxs = txs.Where(
                (tx, index) =>
                    txResults.Data.Transaction.TransactionResults[index].TxStatus
                    == TxStatus.Success
            )
            .ToList();

        return successfulTxs;
    }

    private async void ProcessRootStateDiff(
        IGetDiffs_Diffs_RootStateDiff rootDiff,
        IEnumerable<IGetTransactionSigners_Transaction_NcTransactions> txs
    )
    {
        var accountAddress = new Address(rootDiff.Path);
        if (AddressHandlerMappings.HandlerMappings.TryGetValue(accountAddress, out var handler))
        {
            foreach (var subDiff in rootDiff.Diffs)
            {
                if (subDiff.ChangedState is not null)
                {
                    try
                    {
                        var stateData = handler.ConvertToStateData(
                            new()
                            {
                                Address = new Address(subDiff.Path),
                                RawState = Codec.Decode(
                                    Convert.FromHexString(subDiff.ChangedState)
                                ),
                                Transactions = txs
                            }
                        );

                        if (
                            CollectionNames.CollectionMappings.TryGetValue(
                                stateData.State.GetType(),
                                out var collectionName
                            )
                        )
                        {
                            await _store.UpsertStateDataAsync(stateData, collectionName);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                }
            }
        }
    }

    private void ProcessStateDiff(
        IGetDiffs_Diffs_StateDiff stateDiff,
        IEnumerable<IGetTransactionSigners_Transaction_NcTransactions> txs
    ) { }
}
