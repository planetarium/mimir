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
            if (diffResult.Data?.Diffs != null)
            {
                foreach (var diff in diffResult.Data.Diffs)
                {
                    switch (diff)
                    {
                        case IGetDiffs_Diffs_RootStateDiff rootDiff:
                            ProcessRootStateDiff(rootDiff);
                            break;

                        case IGetDiffs_Diffs_StateDiff stateDiff:
                            ProcessStateDiff(stateDiff);
                            break;
                    }
                }
            }

            await _store.UpdateLatestBlockIndex(currentTargetIndex);
            currentBaseIndex = currentTargetIndex;
            indexDifference -= 9;
        }
    }

    private async void ProcessRootStateDiff(
        IGetDiffs_Diffs_RootStateDiff rootDiff
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
                                )
                            }
                        );
                        await handler.StoreStateData(_store, stateData);
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
        IGetDiffs_Diffs_StateDiff stateDiff
    ) { }
}
