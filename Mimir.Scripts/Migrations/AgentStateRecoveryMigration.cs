using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using Mimir.Worker.Extensions;
using Mimir.Scripts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Text.Json;
using Libplanet.Crypto;
using Lib9c.Models.Extensions;
using Mimir.MongoDB;

namespace Mimir.Scripts.Migrations;

public class AgentStateRecoveryMigration
{
    private readonly IMongoDbService _mongoDbService;
    private readonly IHeadlessGQLClient _headlessGqlClient;
    private readonly IStateService _stateService;
    private readonly ILogger<AgentStateRecoveryMigration> _logger;
    private readonly Worker.Configuration _configuration;
    private const string ProgressFileName = "agent_state_recovery_progress.json";
    private const int LIMIT = 100;

    public AgentStateRecoveryMigration(
        IMongoDbService mongoDbService,
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

    public async Task<int> ExecuteAsync()
    {
        _logger.LogInformation("Agent State 복구 마이그레이션 시작");

        var progress = await LoadProgressAsync();
        var totalTransactions = await GetTotalTransactionCountAsync();

        _logger.LogInformation(
            "복구 대상: 총 {TotalTransactions}개 Transaction",
            totalTransactions
        );

        if (totalTransactions == 0)
        {
            _logger.LogInformation("복구할 Transaction이 없습니다.");
            return 0;
        }

        var totalProcessedTransactions = 0;
        var totalProcessedAgents = 0;
        var processedCount = 0;

        var collection = _mongoDbService.GetCollection<TransactionDocument>(CollectionNames.GetCollectionName<TransactionDocument>());
        var filter = Builders<TransactionDocument>.Filter.Empty;
        var sort = Builders<TransactionDocument>.Sort.Descending("BlockIndex");
        
        using var cursor = await collection.Find(filter).Sort(sort).ToCursorAsync();

        while (await cursor.MoveNextAsync())
        {
            foreach (var transaction in cursor.Current)
            {
                try
                {
                    var signer = transaction.Object.Signer;
                    var isAgentExists = await _mongoDbService.IsExistAgentAsync(signer);
                    if (isAgentExists)
                    {
                        _logger.LogDebug("Agent {Signer}는 이미 존재합니다. 건너뜁니다.", signer.ToHex());
                        totalProcessedTransactions++;
                        processedCount++;
                        continue;
                    }

                    await InsertAgent(signer, transaction.BlockIndex);
                    totalProcessedAgents++;
                    totalProcessedTransactions++;
                    processedCount++;

                    if (processedCount % LIMIT == 0)
                    {
                        progress.ProcessedTransactions = totalProcessedTransactions;
                        progress.ProcessedAgents = totalProcessedAgents;
                        await SaveProgressAsync(progress);

                        _logger.LogInformation(
                            "진행상황: {ProcessedCount}/{TotalTransactions} Transaction 처리됨, 총 {TotalAgents}개 Agent 처리됨",
                            processedCount,
                            totalTransactions,
                            totalProcessedAgents
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Transaction 처리 중 오류 발생");
                }
            }
        }

        progress.ProcessedTransactions = totalProcessedTransactions;
        progress.ProcessedAgents = totalProcessedAgents;
        await SaveProgressAsync(progress);

        _logger.LogInformation(
            "Agent State 복구 마이그레이션 완료. 총 {Transactions}개 Transaction, {Agents}개 Agent 처리됨",
            totalProcessedTransactions,
            totalProcessedAgents
        );

        return totalProcessedAgents;
    }

    private async Task<long> GetTotalTransactionCountAsync()
    {
        var collection = _mongoDbService.GetCollection<TransactionDocument>(CollectionNames.GetCollectionName<TransactionDocument>());
        return await collection.CountDocumentsAsync(Builders<TransactionDocument>.Filter.Empty);
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
                    "진행상황 파일 로드 완료: ProcessedTransactions={ProcessedTransactions}, ProcessedAgents={ProcessedAgents}",
                    progress?.ProcessedTransactions,
                    progress?.ProcessedAgents
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
                "진행상황 저장 완료: ProcessedTransactions={ProcessedTransactions}, ProcessedAgents={ProcessedAgents}",
                progress.ProcessedTransactions,
                progress.ProcessedAgents
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
    public int ProcessedTransactions { get; set; } = 0;
    public int ProcessedAgents { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
} 