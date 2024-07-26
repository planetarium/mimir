using Bencodex;
using HeadlessGQL;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Constants;
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
        long currentTargetIndex = syncedBlockIndex + (indexDifference > 9 ? 9 : indexDifference);

        _logger.Information(
            "Process diff, current&sync: {CurrentBlockIndex}&{SyncedBlockIndex}, index-diff: {IndexDiff}, currentTargetIndex: {CurrentTargetIndex}",
            currentBlockIndex,
            syncedBlockIndex,
            indexDifference,
            currentTargetIndex
        );

        var diffResult = await _headlessGqlClient.GetDiffs.ExecuteAsync(
            syncedBlockIndex,
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

        var tasks = new List<Task>();

        foreach (var diff in diffResult.Data.Diffs)
        {
            switch (diff)
            {
                case IGetDiffs_Diffs_RootStateDiff rootDiff:
                    tasks.Add(ProcessRootStateDiff(rootDiff));
                    break;

                case IGetDiffs_Diffs_StateDiff stateDiff:
                    // await ProcessStateDiff(stateDiff);
                    break;
            }
        }

        await Task.WhenAll(tasks);

        await _store.UpdateLatestBlockIndex(currentTargetIndex, _pollerType);
    }

    private async Task ProcessRootStateDiff(IGetDiffs_Diffs_RootStateDiff rootDiff)
    {
        var accountAddress = new Address(rootDiff.Path);
        if (AddressHandlerMappings.HandlerMappings.TryGetValue(accountAddress, out var handler))
        {
            // var stateDatas = new List<MongoDbCollectionDocument>();
            var documents = new List<IMimirBsonDocument>();
            foreach (var subDiff in rootDiff.Diffs)
            {
                if (subDiff.ChangedState is not null)
                {
                    _logger.Information(
                        "{Handler}: Handle {Address}",
                        handler.GetType().Name,
                        subDiff.Path
                    );
                    var address = new Address(subDiff.Path);

                    var document = handler.ConvertToState(
                        new()
                        {
                            Address = address,
                            RawState = Codec.Decode(Convert.FromHexString(subDiff.ChangedState))
                        }
                    );

                    // stateDatas.Add(new MongoDbCollectionDocument(address, document));
                    documents.Add(document);
                }
            }

            if (documents.Count > 0)
            {
                await _store.UpsertStateDataManyAsync(
                    CollectionNames.GetCollectionName(accountAddress),
                    documents
                );
            }
        }
    }

    private async Task ProcessStateDiff(IGetDiffs_Diffs_StateDiff stateDiff) { }
}
