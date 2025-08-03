using Bencodex;
using Lib9c.Models.Extensions;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Libplanet.Types.Tx;
using Microsoft.Extensions.Options;
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
    IStateService stateService,
    IOptions<Configuration> configuration
) : BackgroundService
{
    public const string PollerType = "BlockPoller";
    public const string collectionName = "block";
    public const string transactionCollectionName = "transaction";
    private readonly StateGetter StateGetter = stateService.At(configuration);
    private const int LIMIT = 50;

    private readonly ILogger Logger = Log.ForContext<BlockHandler>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var (currentBaseIndex, targetLimit) = await CalculateCurrentAndTargetIndexes(
                    stoppingToken
                );

                if (targetLimit < 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(8), stoppingToken);
                    continue;
                }

                Logger.Information(
                    "Request block data, current: {CurrentBlockIndex}, targetLimit: {TargetLimit}",
                    currentBaseIndex,
                    targetLimit
                );

                var (blockResponse, _) = await headlessGqlClient.GetBlocksAsync(
                    (int)currentBaseIndex + 1,
                    (int)targetLimit,
                    stoppingToken
                );

                var blockDocuments = new List<BlockDocument>();
                var transactionDocuments = new List<TransactionDocument>();

                foreach (var block in blockResponse.BlockQuery.Blocks)
                {
                    var blockModel = block.ToBlockModel();
                    var blockDocument = new BlockDocument(
                        blockModel.Index,
                        blockModel.Hash,
                        blockModel
                    );
                    blockDocuments.Add(blockDocument);

                    if (block.Transactions != null)
                    {
                        foreach (var transaction in block.Transactions)
                        {
                            var transactionModel = transaction.ToTransactionModel();
                            var extractedActionValues = ActionParser.ExtractActionValue(
                                transactionModel.Actions[0].Raw
                            );
                            await dbService.UpsertActionTypeAsync(extractedActionValues.TypeId);

                            var transactionDocument = new TransactionDocument(
                                blockModel.Index,
                                transactionModel.Id,
                                blockModel.Hash,
                                blockModel.Index,
                                blockModel.Timestamp,
                                extractedActionValues,
                                transactionModel
                            );
                            transactionDocuments.Add(transactionDocument);
                        }
                    }
                }

                if (blockDocuments.Count < 1)
                {
                    throw new Exception("blockDocuments is empty");
                }

                await dbService.UpsertBlocksManyAsync(blockDocuments);
                long latestBlockIndex = blockDocuments.Max(bd => bd.Object.Index);

                if (transactionDocuments.Count > 0)
                {
                    await UpdateTransactionStatuses(transactionDocuments, stoppingToken);
                    await dbService.UpsertTransactionsManyAsync(transactionDocuments);
                }

                foreach (var document in blockDocuments)
                {
                    Logger.Information(
                        "Processing block index: {BlockIndex}",
                        document.Object.Index
                    );
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
                                    transaction.BlockIndex
                                );
                                await InsertNCGBalanceIfNotExist(
                                    transaction.Object.Signer,
                                    transaction.BlockIndex
                                );

                                foreach (var action in transaction.Object.Actions)
                                {
                                    var extractedActionValues = ActionParser.ExtractActionValue(
                                        action.Raw
                                    );
                                    if (extractedActionValues.InvolvedAvatarAddresses is not null)
                                    {
                                        foreach (
                                            var avatarAddress in extractedActionValues.InvolvedAvatarAddresses
                                        )
                                        {
                                            await InsertAvatarIfNotExist(
                                                avatarAddress,
                                                transaction.BlockIndex
                                            );
                                            await InsertDailyRewardIfNotExist(
                                                avatarAddress,
                                                transaction.BlockIndex
                                            );
                                        }
                                    }
                                    if (extractedActionValues.AvatarAddress is not null)
                                    {
                                        await InsertAvatarIfNotExist(
                                            extractedActionValues.AvatarAddress.Value,
                                            transaction.BlockIndex
                                        );
                                        await InsertDailyRewardIfNotExist(
                                            extractedActionValues.AvatarAddress.Value,
                                            transaction.BlockIndex
                                        );
                                    }
                                }
                            }
                        }
                    }
                }

                Logger.Information(
                    "Synced Latest block index: {LatestBlockIndex}",
                    latestBlockIndex
                );

                await dbService.UpdateLatestBlockIndexAsync(
                    new MetadataDocument
                    {
                        PollerType = PollerType,
                        CollectionName = collectionName,
                        LatestBlockIndex = latestBlockIndex,
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

    private async Task<(long CurrentBaseIndex, long TargetLimit)> CalculateCurrentAndTargetIndexes(
        CancellationToken stoppingToken
    )
    {
        var syncedBlockIndex = await GetSyncedBlockIndex(stoppingToken);
        Logger.Information(
            "{CollectionName} Synced BlockIndex: {SyncedBlockIndex}",
            collectionName,
            syncedBlockIndex
        );

        var currentIndexOnChain = await GetLatestIndex(stoppingToken);
        var indexDifference = currentIndexOnChain - syncedBlockIndex;
        var targetLimit = indexDifference > LIMIT ? LIMIT : indexDifference;

        return (syncedBlockIndex, targetLimit);
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

    private async Task InsertNCGBalanceIfNotExist(Address signer, long blockIndex)
    {
        var isExist = await dbService.IsExistNCGBalanceAsync(signer);
        if (isExist)
            return;

        await InsertNCGBalance(signer, blockIndex);
    }

    private async Task InsertNCGBalance(Address signer, long blockIndex)
    {
        try
        {
            var ncgBalanceState = await StateGetter.GetNCGBalanceAsync(signer);

            var document = new BalanceDocument(blockIndex, signer, ncgBalanceState);

            await dbService.UpsertStateDataManyAsync("balance_ncg", [document]);
        }
        catch (StateNotFoundException)
        {
            var document = new BalanceDocument(blockIndex, signer, "0");
            await dbService.UpsertStateDataManyAsync("balance_ncg", [document]);
        }
    }

    private async Task InsertDailyRewardIfNotExist(Address signer, long blockIndex)
    {
        var isExist = await dbService.IsExistDailyRewardAsync(signer);
        if (isExist)
            return;

        await InsertDailyReward(signer, blockIndex);
    }

    private async Task InsertDailyReward(Address avatarAddress, long blockIndex)
    {
        try
        {
            var dailyRewardState = await StateGetter.GetDailyRewardAsync(avatarAddress);

            var document = new DailyRewardDocument(blockIndex, avatarAddress, dailyRewardState);

            await dbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<DailyRewardDocument>(),
                [document]
            );
        }
        catch (StateNotFoundException)
        {
            var document = new DailyRewardDocument(blockIndex, avatarAddress, 0);
            await dbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<DailyRewardDocument>(),
                [document]
            );
        }
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
        try
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
        catch (StateNotFoundException)
        {
            Logger.Information(
                "Avatar state not found, block index: {BlockIndex}, avatar address: {AvatarAddress}",
                blockIndex,
                avatarAddress
            );
        }
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

    private static Lib9c.Models.Block.TxStatus ConvertToLib9cTxStatus(
        Lib9c.Models.Block.TxStatus clientStatus
    )
    {
        return clientStatus;
    }
}
