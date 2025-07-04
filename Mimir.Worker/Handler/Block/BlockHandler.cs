using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Extensions;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Mimir.Worker.Util;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Handler;

public class BlockHandler(MongoDbService dbService, IHeadlessGQLClient headlessGqlClient)
    : BackgroundService
{
    public const string PollerType = "BlockPoller";
    public const string collectionName = "block";
    private static readonly Codec Codec = new();
    private const int LIMIT = 15;

    private readonly ILogger Logger = Log.ForContext<BlockHandler>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var (currentBaseIndex, currentTargetIndex, currentIndexOnChain, indexDifference) =
                    await CalculateCurrentAndTargetIndexes(stoppingToken);

                if (currentBaseIndex >= currentTargetIndex)
                {
                    await Task.Delay(TimeSpan.FromSeconds(8), stoppingToken);
                    continue;
                }

                Logger.Information(
                    "{CollectionName} Request block data, current: {CurrentBlockIndex}, gap: {IndexDiff}, base: {CurrentBaseIndex} target: {CurrentTargetIndex}",
                    collectionName,
                    currentIndexOnChain,
                    indexDifference,
                    currentBaseIndex,
                    currentTargetIndex
                );

                var (blockResponse, _) = await headlessGqlClient.GetBlocksAsync(
                    (int)currentTargetIndex,
                    LIMIT,
                    stoppingToken
                );

                var documents = new List<BlockDocument>();
                foreach (var block in blockResponse.BlockQuery.Blocks)
                {
                    var blockModel = block.ToBlockModel();
                    var document = new BlockDocument(
                        currentTargetIndex,
                        blockModel.Hash,
                        blockModel
                    );
                    documents.Add(document);
                }

                if (documents.Count > 0)
                    await dbService.InsertBlocksManyAsync(documents);

                await dbService.UpdateLatestBlockIndexAsync(
                    new MetadataDocument
                    {
                        PollerType = PollerType,
                        CollectionName = "block",
                        LatestBlockIndex = currentTargetIndex,
                    },
                    null,
                    stoppingToken
                );
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unexpected error occurred.");
            }
        }
    }

    protected virtual async Task<(
        long CurrentBaseIndex,
        long CurrentTargetIndex,
        long CurrentIndexOnChain,
        long IndexDifference
    )> CalculateCurrentAndTargetIndexes(CancellationToken stoppingToken)
    {
        var syncedIndex = await GetSyncedBlockIndex(stoppingToken);
        var currentBaseIndex = syncedIndex;
        Logger.Information(
            "{CollectionName} Synced BlockIndex: {SyncedBlockIndex}",
            collectionName,
            syncedIndex
        );

        var currentIndexOnChain = await GetLatestIndex(stoppingToken);
        var indexDifference = currentIndexOnChain - currentBaseIndex;
        var currentTargetIndex =
            currentBaseIndex + (indexDifference > LIMIT ? LIMIT : indexDifference);

        return (currentBaseIndex, currentTargetIndex, currentIndexOnChain, indexDifference);
    }

    protected virtual async Task<long> GetSyncedBlockIndex(CancellationToken stoppingToken)
    {
        try
        {
            var syncedBlockIndex = await dbService.GetLatestBlockIndexAsync(
                PollerType,
                collectionName,
                stoppingToken
            );
            return syncedBlockIndex;
        }
        catch (InvalidOperationException)
        {
            var currentBlockIndex = await GetLatestIndex(stoppingToken);
            Logger.Information(
                "Metadata collection is not found, set block index to {BlockIndex} - 1",
                currentBlockIndex
            );
            await dbService.UpdateLatestBlockIndexAsync(
                new MetadataDocument
                {
                    PollerType = PollerType,
                    CollectionName = collectionName,
                    LatestBlockIndex = currentBlockIndex - 1,
                },
                cancellationToken: stoppingToken
            );
            return currentBlockIndex - 1;
        }
    }

    private async Task<long> GetLatestIndex(CancellationToken stoppingToken = default)
    {
        var (result, _) = await headlessGqlClient.GetTipAsync(stoppingToken, null);
        return result.NodeStatus.Tip.Index;
    }
}
