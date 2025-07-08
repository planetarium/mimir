using Bencodex;
using Lib9c.Models.Extensions;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Extensions;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Handler;

public class BlockHandler(
    MongoDbService dbService,
    IHeadlessGQLClient headlessGqlClient,
    IStateService stateService
) : BackgroundService
{
    public const string PollerType = "BlockPoller";
    public const string collectionName = "block";
    private readonly StateGetter StateGetter = stateService.At();
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
                        CollectionName = collectionName,
                        LatestBlockIndex = currentTargetIndex,
                    },
                    null,
                    stoppingToken
                );

                foreach (var document in documents)
                {
                    foreach (var transaction in document.Object.Transactions)
                    {
                        await InsertAgentIfNotExist(
                            new Address(transaction.Signer),
                            currentTargetIndex
                        );

                        foreach (var action in transaction.Actions)
                        {
                            var avatarAddresses = ActionParser.ExtractAvatarAddress(action.Raw);
                            foreach (var avatarAddress in avatarAddresses)
                            {
                                await InsertAvatarIfNotExist(avatarAddress, currentTargetIndex);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unexpected error occurred.");
            }
        }
    }

    private async Task<(
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

    private async Task<long> GetSyncedBlockIndex(CancellationToken stoppingToken)
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

    private async Task InsertAgentIfNotExist(Address signer, long blockIndex)
    {
        var isExist = await dbService.IsExistAgentAsync(signer);
        if (isExist)
            return;

        await InsertAgent(signer, blockIndex);
    }

    private async Task InsertAgent(Address signer, long blockIndex)
    {
        try
        {
            var agentState = await StateGetter.GetAgentStateAccount(signer);

            var document = new AgentDocument(blockIndex, agentState.Address, agentState);

            await dbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AgentDocument>(),
                [document]
            );
        }
        catch (StateNotFoundException)
        {
            var document = new AgentDocument(blockIndex, signer, new AgentState());
            await dbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AgentDocument>(),
                [document]
            );
        }
    }

    private async Task InsertAvatarIfNotExist(Address avatarAddress, long blockIndex)
    {
        var isExist = await dbService.IsExistAvatarAsync(avatarAddress);
        if (isExist)
            return;

        await InsertAvatar(avatarAddress, blockIndex);
    }

    private async Task InsertAvatar(Address avatarAddress, long blockIndex)
    {
        var avatarState = await StateGetter.GetAvatarStateAsync(avatarAddress);
        var inventoryState = await StateGetter.GetInventoryState(
            avatarAddress,
            CancellationToken.None
        );
        var armorId = inventoryState.GetArmorId();
        var portraitId = inventoryState.GetPortraitId();

        var document = new AvatarDocument(
            blockIndex,
            avatarState.Address,
            avatarState,
            armorId,
            portraitId
        );

        await dbService.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<AvatarDocument>(),
            [document]
        );
    }
}
