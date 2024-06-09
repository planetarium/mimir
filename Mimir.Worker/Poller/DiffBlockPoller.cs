using Bencodex;
using HeadlessGQL;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Services;

namespace Mimir.Worker.Poller;

public class DiffBlockPoller : BaseBlockPoller
{
    private readonly HeadlessGQLClient _headlessGqlClient;
    private readonly Codec Codec = new();

    public DiffBlockPoller(
        ILogger<DiffBlockPoller> logger,
        IStateService stateService,
        HeadlessGQLClient headlessGqlClient,
        DiffMongoDbService store
    )
        : base(logger, stateService, store, "DiffBlockPoller")
    {
        _headlessGqlClient = headlessGqlClient;
    }

    protected override async Task ProcessBlocksAsync(
        long syncedBlockIndex,
        long currentBlockIndex,
        CancellationToken stoppingToken
    )
    {
        long indexDifference = Math.Abs(syncedBlockIndex - currentBlockIndex);

        long currentBaseIndex = syncedBlockIndex;

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

                await _store.UpdateLatestBlockIndex(currentTargetIndex, _pollerType);
                currentBaseIndex = currentTargetIndex;
                indexDifference -= 9;
            }
        }
    }

    private async void ProcessRootStateDiff(IGetDiffs_Diffs_RootStateDiff rootDiff)
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
                                RawState = Codec.Decode(Convert.FromHexString(subDiff.ChangedState))
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

    private void ProcessStateDiff(IGetDiffs_Diffs_StateDiff stateDiff) { }
}
