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

namespace Mimir.Scripts.Migrations;

public class AgentStateRecoveryMigration
{
    private readonly MongoDbService _mongoDbService;
    private readonly IHeadlessGQLClient _headlessGqlClient;
    private readonly IStateService _stateService;
    private readonly ILogger<AgentStateRecoveryMigration> _logger;
    private readonly Worker.Configuration _configuration;
    private const string ProgressFileName = "agent_state_recovery_progress.json";
    private const int LIMIT = 100;

    public AgentStateRecoveryMigration(
        MongoDbService mongoDbService,
        IHeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        ILogger<AgentStateRecoveryMigration> logger,
        IOptions<Worker.Configuration> configuration
    )
    {
        _mongoDbService = mongoDbService;
        _headlessGqlClient = headlessGqlClient;
        _stateService = stateService;
        _logger = logger;
        _configuration = configuration.Value;
    }

    public async Task<int> ExecuteAsync(long startBlockIndex, long? endBlockIndex = null)
    {
        _logger.LogInformation("Agent State 복구 마이그레이션 시작. 시작 블록 인덱스: {StartBlockIndex}, 끝 블록 인덱스: {EndBlockIndex}", startBlockIndex, endBlockIndex);

        var progress = await LoadProgressAsync();
        var currentBlockIndex = Math.Max(startBlockIndex, progress.LastProcessedBlockIndex);
        var latestBlockIndex = await GetLatestBlockIndexAsync();
        var targetEndBlockIndex = endBlockIndex ?? latestBlockIndex;

        _logger.LogInformation(
            "복구 범위: {CurrentBlockIndex} ~ {TargetEndBlockIndex} (최신 블록: {LatestBlockIndex})",
            currentBlockIndex,
            targetEndBlockIndex,
            latestBlockIndex
        );

        if (currentBlockIndex >= targetEndBlockIndex)
        {
            _logger.LogInformation("복구할 블록이 없습니다.");
            return 0;
        }

        var totalProcessedBlocks = 0;
        var totalProcessedAgents = 0;

        while (currentBlockIndex < targetEndBlockIndex)
        {
            try
            {
                var targetBlockIndex = Math.Min(currentBlockIndex + LIMIT, targetEndBlockIndex);
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

                var processedAgentsInBatch = 0;

                foreach (var block in blockResponse.BlockQuery.Blocks)
                {
                    var blockModel = block.ToBlockModel();
                    
                    if (block.Transactions != null)
                    {
                        foreach (var transaction in block.Transactions)
                        {
                            var transactionModel = transaction.ToTransactionModel();
                            var signer = transactionModel.Signer;
                            
                            var isAgentExists = await _mongoDbService.IsExistAgentAsync(signer);
                            if (isAgentExists)
                            {
                                _logger.LogDebug("Agent {Signer}는 이미 존재합니다. 건너뜁니다.", signer.ToHex());
                                continue;
                            }

                            await InsertAgent(signer, blockModel.Index);
                            processedAgentsInBatch++;
                            totalProcessedAgents++;
                        }
                    }
                }

                totalProcessedBlocks += blockResponse.BlockQuery.Blocks.Count;

                currentBlockIndex = targetBlockIndex;
                progress.LastProcessedBlockIndex = currentBlockIndex;
                progress.ProcessedBlocks = totalProcessedBlocks;
                progress.ProcessedAgents = totalProcessedAgents;
                await SaveProgressAsync(progress);

                _logger.LogInformation(
                    "배치 처리 완료. 현재 진행률: {CurrentBlockIndex}/{TargetEndBlockIndex}, 이번 배치에서 {ProcessedAgents}개 Agent 처리됨",
                    currentBlockIndex,
                    targetEndBlockIndex,
                    processedAgentsInBatch
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "블록 배치 처리 중 오류 발생. 블록 인덱스: {BlockIndex}", currentBlockIndex);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        _logger.LogInformation(
            "Agent State 복구 마이그레이션 완료. 총 {Blocks}개 블록, {Agents}개 Agent 처리됨 (범위: {StartBlockIndex} ~ {EndBlockIndex})",
            totalProcessedBlocks,
            totalProcessedAgents,
            startBlockIndex,
            targetEndBlockIndex
        );

        return totalProcessedAgents;
    }

    private async Task<long> GetLatestBlockIndexAsync()
    {
        var (result, _) = await _headlessGqlClient.GetTipAsync(CancellationToken.None, null);
        return result.NodeStatus.Tip.Index;
    }

    private async Task InsertAgent(Address signer, long blockIndex)
    {
        var stateGetter = _stateService.At(new OptionsWrapper<Worker.Configuration>(_configuration));
        
        try
        {
            var agentState = await stateGetter.GetAgentStateAccount(signer);
            var document = new AgentDocument(blockIndex, agentState.Address, agentState);
            await _mongoDbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AgentDocument>(),
                [document]
            );
            _logger.LogDebug("Agent State 삽입 완료: {Signer}", signer.ToHex());
        }
        catch (Mimir.Worker.Exceptions.StateNotFoundException)
        {
            var document = new AgentDocument(blockIndex, signer, new Lib9c.Models.States.AgentState());
            await _mongoDbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AgentDocument>(),
                [document]
            );
            _logger.LogDebug("Agent State (빈 상태) 삽입 완료: {Signer}", signer.ToHex());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent State 삽입 중 오류 발생: {Signer}", signer.ToHex());
        }
    }

    private async Task<AgentStateRecoveryProgress> LoadProgressAsync()
    {
        if (File.Exists(ProgressFileName))
        {
            try
            {
                var json = await File.ReadAllTextAsync(ProgressFileName);
                var progress = JsonSerializer.Deserialize<AgentStateRecoveryProgress>(json);
                _logger.LogInformation(
                    "진행상황 파일 로드 완료: LastProcessedBlockIndex={LastProcessedBlockIndex}",
                    progress?.LastProcessedBlockIndex
                );
                return progress ?? new AgentStateRecoveryProgress();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "진행상황 파일 로드 실패: {FileName}", ProgressFileName);
            }
        }

        var newProgress = new AgentStateRecoveryProgress();
        await SaveProgressAsync(newProgress);
        return newProgress;
    }

    private async Task SaveProgressAsync(AgentStateRecoveryProgress progress)
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

public class AgentStateRecoveryProgress
{
    public long LastProcessedBlockIndex { get; set; } = 0;
    public int ProcessedBlocks { get; set; } = 0;
    public int ProcessedAgents { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
} 