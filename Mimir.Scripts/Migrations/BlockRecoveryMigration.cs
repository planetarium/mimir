using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using Mimir.Worker.Extensions;
using Mimir.Scripts;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using Libplanet.Crypto;
using Lib9c.Models.Extensions;
using Options = Microsoft.Extensions.Options.Options;

namespace Mimir.Scripts.Migrations;

public class BlockRecoveryMigration
{
    private readonly MongoDbService _mongoDbService;
    private readonly IHeadlessGQLClient _headlessGqlClient;
    private readonly IStateService _stateService;
    private readonly ILogger<BlockRecoveryMigration> _logger;
    private readonly Worker.Configuration _configuration;
    private const string ProgressFileName = "block_recovery_progress.json";
    private const int LIMIT = 50;

    public BlockRecoveryMigration(
        MongoDbService mongoDbService,
        IHeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        ILogger<BlockRecoveryMigration> logger,
        IOptions<Worker.Configuration> configuration
    )
    {
        _mongoDbService = mongoDbService;
        _headlessGqlClient = headlessGqlClient;
        _stateService = stateService;
        _logger = logger;
        _configuration = configuration.Value;
    }

    public async Task<int> ExecuteAsync(long startBlockIndex)
    {
        _logger.LogInformation("블록 복구 마이그레이션 시작. 시작 블록 인덱스: {StartBlockIndex}", startBlockIndex);

        var progress = await LoadProgressAsync();
        var currentBlockIndex = Math.Max(startBlockIndex, progress.LastProcessedBlockIndex);
        var latestBlockIndex = await GetLatestBlockIndexAsync();

        _logger.LogInformation(
            "복구 범위: {CurrentBlockIndex} ~ {LatestBlockIndex}",
            currentBlockIndex,
            latestBlockIndex
        );

        if (currentBlockIndex >= latestBlockIndex)
        {
            _logger.LogInformation("복구할 블록이 없습니다.");
            return 0;
        }

        var totalProcessedBlocks = 0;
        var totalProcessedTransactions = 0;

        while (currentBlockIndex < latestBlockIndex)
        {
            try
            {
                var targetBlockIndex = Math.Min(currentBlockIndex + LIMIT, latestBlockIndex);
                var batchSize = (int)(targetBlockIndex - currentBlockIndex);

                _logger.LogInformation(
                    "블록 배치 처리 중: {CurrentBlockIndex} ~ {TargetBlockIndex} ({BatchSize}개)",
                    currentBlockIndex,
                    targetBlockIndex,
                    batchSize
                );

                var (blockResponse, _) = await _headlessGqlClient.GetBlocksAsync(
                    (int)currentBlockIndex,
                    batchSize,
                    CancellationToken.None
                );

                var blockDocuments = new List<BlockDocument>();
                var transactionDocuments = new List<TransactionDocument>();

                foreach (var block in blockResponse.BlockQuery.Blocks)
                {
                    var blockModel = block.ToBlockModel();
                    
                    var isBlockExists = await _mongoDbService.IsExistBlockAsync(blockModel.Index);
                    if (isBlockExists)
                    {
                        _logger.LogDebug("블록 {BlockIndex}는 이미 존재합니다. 건너뜁니다.", blockModel.Index);
                        continue;
                    }

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
                            await _mongoDbService.UpsertActionTypeAsync(extractedActionValues.TypeId);

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

                if (blockDocuments.Count > 0)
                {
                    await _mongoDbService.InsertBlocksManyAsync(blockDocuments);
                    _logger.LogInformation(
                        "{Count}개 블록 문서 삽입 완료",
                        blockDocuments.Count
                    );
                    totalProcessedBlocks += blockDocuments.Count;
                }

                if (transactionDocuments.Count > 0)
                {
                    await UpdateTransactionStatuses(transactionDocuments);
                    await _mongoDbService.InsertTransactionsManyAsync(transactionDocuments);
                    _logger.LogInformation(
                        "{Count}개 트랜잭션 문서 삽입 완료",
                        transactionDocuments.Count
                    );
                    totalProcessedTransactions += transactionDocuments.Count;
                }

                await ProcessRelatedStates(blockDocuments, transactionDocuments);

                currentBlockIndex = targetBlockIndex;
                progress.LastProcessedBlockIndex = currentBlockIndex;
                progress.ProcessedBlocks = totalProcessedBlocks;
                progress.ProcessedTransactions = totalProcessedTransactions;
                await SaveProgressAsync(progress);

                _logger.LogInformation(
                    "배치 처리 완료. 현재 진행률: {CurrentBlockIndex}/{LatestBlockIndex}",
                    currentBlockIndex,
                    latestBlockIndex
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "블록 배치 처리 중 오류 발생. 블록 인덱스: {BlockIndex}", currentBlockIndex);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        _logger.LogInformation(
            "블록 복구 마이그레이션 완료. 총 {Blocks}개 블록, {Transactions}개 트랜잭션 처리됨",
            totalProcessedBlocks,
            totalProcessedTransactions
        );

        return totalProcessedBlocks;
    }

    private async Task<long> GetLatestBlockIndexAsync()
    {
        var (result, _) = await _headlessGqlClient.GetTipAsync(CancellationToken.None, null);
        return result.NodeStatus.Tip.Index;
    }

    private async Task ProcessRelatedStates(
        List<BlockDocument> blockDocuments,
        List<TransactionDocument> transactionDocuments
    )
    {
        var stateGetter = _stateService.At(new OptionsWrapper<Worker.Configuration>(_configuration));

        foreach (var document in blockDocuments)
        {
            if (document.Object.TxIds != null)
            {
                foreach (var txId in document.Object.TxIds)
                {
                    var transaction = transactionDocuments.FirstOrDefault(td => td.TxId == txId);
                    if (transaction != null)
                    {
                        await InsertAgent(transaction.Object.Signer, document.StoredBlockIndex, stateGetter);
                        await InsertNCGBalanceIfNotExist(
                            transaction.Object.Signer,
                            document.StoredBlockIndex,
                            stateGetter
                        );

                        foreach (var action in transaction.Object.Actions)
                        {
                            var extractedActionValues = ActionParser.ExtractActionValue(action.Raw);
                            if (extractedActionValues.InvolvedAvatarAddresses is not null)
                            {
                                foreach (var avatarAddress in extractedActionValues.InvolvedAvatarAddresses)
                                {
                                    await InsertAvatar(avatarAddress, document.StoredBlockIndex, stateGetter);
                                    await InsertDailyRewardIfNotExist(
                                        avatarAddress,
                                        document.StoredBlockIndex,
                                        stateGetter
                                    );
                                }
                            }
                            if (extractedActionValues.AvatarAddress is not null)
                            {
                                await InsertAvatarIfNotExist(
                                    extractedActionValues.AvatarAddress.Value,
                                    document.StoredBlockIndex,
                                    stateGetter
                                );
                                await InsertDailyRewardIfNotExist(
                                    extractedActionValues.AvatarAddress.Value,
                                    document.StoredBlockIndex,
                                    stateGetter
                                );
                            }
                        }
                    }
                }
            }
        }
    }

    private async Task InsertNCGBalanceIfNotExist(Address signer, long blockIndex, StateGetter stateGetter)
    {
        var isExist = await _mongoDbService.IsExistNCGBalanceAsync(signer);
        if (isExist)
            return;

        await InsertNCGBalance(signer, blockIndex, stateGetter);
    }

    private async Task InsertNCGBalance(Address signer, long blockIndex, StateGetter stateGetter)
    {
        try
        {
            var ncgBalanceState = await stateGetter.GetNCGBalanceAsync(signer);
            var document = new BalanceDocument(blockIndex, signer, ncgBalanceState);
            await _mongoDbService.UpsertStateDataManyAsync("balance_ncg", [document]);
        }
        catch (Mimir.Worker.Exceptions.StateNotFoundException)
        {
            var document = new BalanceDocument(blockIndex, signer, "0");
            await _mongoDbService.UpsertStateDataManyAsync("balance_ncg", [document]);
        }
    }

    private async Task InsertDailyRewardIfNotExist(Address signer, long blockIndex, StateGetter stateGetter)
    {
        var isExist = await _mongoDbService.IsExistDailyRewardAsync(signer);
        if (isExist)
            return;

        await InsertDailyReward(signer, blockIndex, stateGetter);
    }

    private async Task InsertDailyReward(Address avatarAddress, long blockIndex, StateGetter stateGetter)
    {
        try
        {
            var dailyRewardState = await stateGetter.GetDailyRewardAsync(avatarAddress);
            var document = new DailyRewardDocument(blockIndex, avatarAddress, dailyRewardState);
            await _mongoDbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<DailyRewardDocument>(),
                [document]
            );
        }
        catch (Mimir.Worker.Exceptions.StateNotFoundException)
        {
            var document = new DailyRewardDocument(blockIndex, avatarAddress, 0);
            await _mongoDbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<DailyRewardDocument>(),
                [document]
            );
        }
    }

    private async Task InsertAgent(Address signer, long blockIndex, StateGetter stateGetter)
    {
        try
        {
            var agentState = await stateGetter.GetAgentStateAccount(signer);
            var document = new AgentDocument(blockIndex, agentState.Address, agentState);
            await _mongoDbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AgentDocument>(),
                [document]
            );
        }
        catch (Mimir.Worker.Exceptions.StateNotFoundException)
        {
            var document = new AgentDocument(blockIndex, signer, new Lib9c.Models.States.AgentState());
            await _mongoDbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AgentDocument>(),
                [document]
            );
        }
    }

    private async Task InsertAvatarIfNotExist(Address avatarAddress, long blockIndex, StateGetter stateGetter)
    {
        var isExist = await _mongoDbService.IsExistAvatarAsync(avatarAddress);
        if (isExist)
            return;

        await InsertAvatar(avatarAddress, blockIndex, stateGetter);
    }

    private async Task InsertAvatar(Address avatarAddress, long blockIndex, StateGetter stateGetter)
    {
        var avatarState = await stateGetter.GetAvatarStateAsync(avatarAddress);
        var inventoryState = await stateGetter.GetInventoryState(avatarAddress, CancellationToken.None);
        var armorId = inventoryState.GetArmorId();
        var portraitId = inventoryState.GetPortraitId();

        var document = new AvatarDocument(
            blockIndex,
            avatarState.Address,
            avatarState,
            armorId,
            portraitId
        );

        await _mongoDbService.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<AvatarDocument>(),
            [document]
        );
    }

    private async Task UpdateTransactionStatuses(List<TransactionDocument> documents)
    {
        var (statusResponse, _) = await _headlessGqlClient.GetTransactionStatusesAsync(
            documents.Select(txDocument => Libplanet.Types.Tx.TxId.FromHex(txDocument.Object.Id)).ToList(),
            CancellationToken.None
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

    private async Task<BlockRecoveryProgress> LoadProgressAsync()
    {
        if (File.Exists(ProgressFileName))
        {
            try
            {
                var json = await File.ReadAllTextAsync(ProgressFileName);
                var progress = JsonSerializer.Deserialize<BlockRecoveryProgress>(json);
                _logger.LogInformation(
                    "진행상황 파일 로드 완료: LastProcessedBlockIndex={LastProcessedBlockIndex}",
                    progress?.LastProcessedBlockIndex
                );
                return progress ?? new BlockRecoveryProgress();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "진행상황 파일 로드 실패: {FileName}", ProgressFileName);
            }
        }

        var newProgress = new BlockRecoveryProgress();
        await SaveProgressAsync(newProgress);
        return newProgress;
    }

    private async Task SaveProgressAsync(BlockRecoveryProgress progress)
    {
        try
        {
            var json = JsonSerializer.Serialize(progress, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(ProgressFileName, json);
            _logger.LogDebug(
                "진행상황 저장 완료: LastProcessedBlockIndex={LastProcessedBlockIndex}",
                progress.LastProcessedBlockIndex
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "진행상황 저장 실패: {FileName}", ProgressFileName);
        }
    }
}

public class BlockRecoveryProgress
{
    public long LastProcessedBlockIndex { get; set; } = 0;
    public int ProcessedBlocks { get; set; } = 0;
    public int ProcessedTransactions { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
} 