using Bencodex;
using Lib9c.Models.Extensions;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Libplanet.Types.Tx;
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
    public const string transactionCollectionName = "transaction";
    private readonly StateGetter StateGetter = stateService.At();
    private const int LIMIT = 50;

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

                var blockDocuments = new List<BlockDocument>();
                var transactionDocuments = new List<TransactionDocument>();

                foreach (var block in blockResponse.BlockQuery.Blocks)
                {
                    var blockModel = block.ToBlockModel();
                    var blockDocument = new BlockDocument(
                        currentTargetIndex,
                        blockModel.Hash,
                        blockModel
                    );
                    blockDocuments.Add(blockDocument);

                    if (block.Transactions != null)
                    {
                        foreach (var transaction in block.Transactions)
                        {
                            var transactionModel = transaction.ToTransactionModel();
                            var (firstActionTypeId, firstAvatarAddress, firstNCGAmount) =
                                ExtractTransactionMetadata(transactionModel);

                            if (firstActionTypeId is not null)
                            {
                                await dbService.UpsertActionTypeAsync(firstActionTypeId);
                            }

                            var transactionDocument = new TransactionDocument(
                                currentTargetIndex,
                                transactionModel.Id,
                                blockModel.Hash,
                                blockModel.Index,
                                firstActionTypeId,
                                firstAvatarAddress,
                                firstNCGAmount,
                                transactionModel
                            );
                            transactionDocuments.Add(transactionDocument);
                        }
                    }
                }

                if (blockDocuments.Count > 0)
                {
                    await dbService.InsertBlocksManyAsync(blockDocuments);
                }

                if (transactionDocuments.Count > 0)
                {
                    await UpdateTransactionStatuses(transactionDocuments, stoppingToken);
                    await dbService.InsertTransactionsManyAsync(transactionDocuments);
                }

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

                foreach (var document in blockDocuments)
                {
                    if (document.Object.TxIds != null)
                    {
                        foreach (var txId in document.Object.TxIds)
                        {
                            var transaction = transactionDocuments.FirstOrDefault(td =>
                                td.TxId == txId
                            );
                            if (transaction != null)
                            {
                                await InsertAgentIfNotExist(
                                    transaction.Object.Signer,
                                    currentTargetIndex
                                );

                                foreach (var action in transaction.Object.Actions)
                                {
                                    var avatarAddresses = ActionParser.ExtractAvatarAddress(
                                        action.Raw
                                    );
                                    foreach (var avatarAddress in avatarAddresses)
                                    {
                                        await InsertAvatarIfNotExist(
                                            avatarAddress,
                                            currentTargetIndex
                                        );
                                    }
                                }
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

    private async Task UpdateTransactionStatuses(
        List<TransactionDocument> documents,
        CancellationToken stoppingToken
    )
    {
        var (statusResponse, _) = await headlessGqlClient.GetTransactionStatusesAsync(
            documents.Select(txDocument => TxId.FromHex(txDocument.Object.Id)).ToList(),
            stoppingToken
        );

        if (statusResponse.Transaction?.TransactionResults != null)
        {
            for (int i = 0; i < documents.Count; i++)
            {
                var status = statusResponse.Transaction.TransactionResults[i];
                documents[i].Object.TxStatus = ConvertToLib9cTxStatus(status.TxStatus);
            }
        }
    }

    private (
        string firstActionTypeId,
        string? firstAvatarAddress,
        string? firstNCGAmount
    ) ExtractTransactionMetadata(Lib9c.Models.Block.Transaction transaction)
    {
        if (transaction.Actions.Count == 0)
        {
            return ("not found", null, null);
        }

        var firstAction = transaction.Actions[0];
        var firstActionTypeId = string.IsNullOrEmpty(firstAction.TypeId)
            ? "not found"
            : firstAction.TypeId;
        var avatarAddress = ActionParser
            .ExtractAvatarAddress(firstAction.Raw)
            .FirstOrDefault(addr => addr != default && !addr.Equals(default));
        var firstAvatarAddress =
            (avatarAddress == null || avatarAddress.Equals(default)) ? null : avatarAddress.ToHex();
        var firstNCGAmount = ActionParser.ExtractNCGAmount(firstAction.Raw);

        return (firstActionTypeId, firstAvatarAddress, firstNCGAmount);
    }

    private static Lib9c.Models.Block.TxStatus ConvertToLib9cTxStatus(
        Lib9c.Models.Block.TxStatus clientStatus
    )
    {
        return clientStatus;
    }
}
