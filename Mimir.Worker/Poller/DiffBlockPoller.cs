using Bencodex;
using HeadlessGQL;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Services;
using Serilog;

namespace Mimir.Worker.Poller;

public class DiffBlockPoller : BaseBlockPoller
{
    private readonly HeadlessGQLClient _headlessGqlClient;
    private readonly Codec Codec = new();

    public DiffBlockPoller(
        IStateService stateService,
        HeadlessGQLClient headlessGqlClient,
        MongoDbService store
    )
        : base(stateService, store, "DiffBlockPoller", Log.ForContext<DiffBlockPoller>())
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
        long currentTargetIndex = currentBaseIndex + (indexDifference > 2 ? 2 : indexDifference);

        _logger.Information(
            "Process diff, current&sync: {CurrentBlockIndex}&{SyncedBlockIndex}, index-diff: {IndexDiff}, currentTargetIndex: {CurrentTargetIndex}",
            currentBlockIndex,
            syncedBlockIndex,
            indexDifference,
            currentTargetIndex
        );

        var diffResult = await _headlessGqlClient.GetDiffs.ExecuteAsync(
            currentBaseIndex,
            currentTargetIndex
        );
        if (diffResult.Data is null)
        {
            var errors = diffResult.Errors.Select(e => e.Message);
            _logger.Error("Failed to get diffs. response data is null. errors:\n{Errors}", errors);
            return;
        }

        _logger.Information(
            "GetDiffs Success, diff-count: {DiffCount}",
            diffResult.Data.Diffs.Count
        );

        foreach (var diff in diffResult.Data.Diffs)
        {
            switch (diff)
            {
                case IGetDiffs_Diffs_RootStateDiff rootDiff:
                    await ProcessRootStateDiff(rootDiff);
                    break;

                case IGetDiffs_Diffs_StateDiff stateDiff:
                    await ProcessStateDiff(stateDiff);
                    break;
            }
        }

        await _store.UpdateLatestBlockIndex(currentTargetIndex, _pollerType);
    }

    private async Task ProcessRootStateDiff(IGetDiffs_Diffs_RootStateDiff rootDiff)
    {
        var accountAddress = new Address(rootDiff.Path);
        if (AddressHandlerMappings.HandlerMappings.TryGetValue(accountAddress, out var handler))
        {
            foreach (var subDiff in rootDiff.Diffs)
            {
                if (subDiff.ChangedState is not null)
                {
                    _logger.Information(
                        "{Handler}: Handle {Address}",
                        handler.GetType().Name,
                        subDiff.Path
                    );

                    var stateData = handler.ConvertToStateData(
                        new()
                        {
                            Address = new Address(subDiff.Path),
                            RawState = Codec.Decode(Convert.FromHexString(subDiff.ChangedState))
                        }
                    );
                    await handler.StoreStateData(_store, stateData);
                }
            }
        }
    }

    private async Task ProcessStateDiff(IGetDiffs_Diffs_StateDiff stateDiff) { }
}
