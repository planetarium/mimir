using Libplanet.Crypto;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Mimir.Worker.Util;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Serilog;
using ILogger = Serilog.ILogger;
using Mimir.MongoDB;

namespace Mimir.Initializer.Initializer;

public class AgentRefreshInitializer : IExecutor
{
    private readonly IMongoDbService _dbService;
    private readonly IStateService _stateService;
    private readonly StateGetter _stateGetter;
    private readonly ILogger _logger;
    private readonly bool _shouldRun;
    private readonly AgentStateDocumentConverter _agentConverter;
    
    public AgentRefreshInitializer(
        IOptions<Configuration> configuration,
        IMongoDbService dbService,
        IStateService stateService)
    {
        _dbService = dbService;
        _stateService = stateService;
        _stateGetter = stateService.At(configuration);
        _shouldRun = configuration.Value.RunOptions.HasFlag(RunOptions.AgentRefreshInitializer);
        _logger = Log.ForContext<AgentRefreshInitializer>();
        _agentConverter = new AgentStateDocumentConverter();
    }
    
    public bool ShouldRun() => _shouldRun;
    
    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        _logger.Information("Starting AgentRefreshInitializer");
        
        try
        {
            // 이미 DB에 있는 에이전트 주소 목록 가져오기
            var existingAgentAddresses = await GetExistingAgentAddresses(stoppingToken);
            _logger.Information("Found {Count} existing agent documents in database", existingAgentAddresses.Count);
            
            // 아바타 컬렉션에서 모든 문서 가져오기
            var avatarCollectionName = CollectionNames.GetCollectionName<AvatarDocument>();
            var avatarCollection = _dbService.GetCollection(avatarCollectionName);
            
            // 배치 크기 설정
            const int batchSize = 100;
            var filter = Builders<BsonDocument>.Filter.Empty;
            var options = new FindOptions<BsonDocument>
            {
                BatchSize = batchSize
            };
            
            using var cursor = await avatarCollection.FindAsync(filter, options, stoppingToken);
            var processedCount = 0;
            var successCount = 0;
            var skipCount = 0;
            var errorCount = 0;
            
            // 에이전트 문서를 저장할 리스트
            var agentDocuments = new List<MimirBsonDocument>();
            var processedAgentAddresses = new HashSet<string>(existingAgentAddresses);
            
            // 커서로 문서 배치 처리
            while (await cursor.MoveNextAsync(stoppingToken))
            {
                foreach (var bsonDoc in cursor.Current)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;
                    
                    processedCount++;
                    
                    try
                    {
                        // BsonDocument를 AvatarDocument로 변환
                        AvatarDocument? avatarDoc = null;
                        try
                        {
                            avatarDoc = BsonSerializer.Deserialize<AvatarDocument>(bsonDoc);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning(ex, "Failed to deserialize BsonDocument to AvatarDocument");
                            errorCount++;
                            continue;
                        }
                        
                        if (avatarDoc == null || avatarDoc.Object == null)
                        {
                            _logger.Warning("AvatarDocument or its Object is null");
                            errorCount++;
                            continue;
                        }
                        
                        var avatarState = avatarDoc.Object;
                        
                        // 이미 처리한 에이전트 주소는 건너뛰기
                        if (processedAgentAddresses.Contains(avatarState.AgentAddress.ToHex()))
                        {
                            skipCount++;
                            continue;
                        }
                        
                        // 에이전트 상태 가져오기
                        var agentState = await _stateGetter.GetStateWithLegacyAccount(
                            avatarState.AgentAddress, 
                            Nekoyume.Addresses.Agent, 
                            stoppingToken);
                        
                        if (agentState != null)
                        {
                            var blockIndex = await _stateService.GetLatestIndex(stoppingToken);
                            
                            // AddressStatePair 생성
                            var pair = new AddressStatePair
                            {
                                BlockIndex = blockIndex,
                                Address = avatarState.AgentAddress,
                                RawState = agentState
                            };
                            
                            // 에이전트 문서 생성
                            var agentDoc = _agentConverter.ConvertToDocument(pair);
                            agentDocuments.Add(agentDoc);
                            processedAgentAddresses.Add(avatarState.AgentAddress.ToHex());
                            successCount++;
                            
                            // 일정 개수마다 배치 저장
                            if (agentDocuments.Count >= batchSize)
                            {
                                await SaveAgentDocuments(agentDocuments, stoppingToken);
                                agentDocuments.Clear();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error processing avatar document");
                        errorCount++;
                    }
                    
                    // 로그 표시
                    if (processedCount % 100 == 0)
                    {
                        _logger.Information(
                            "Processed {ProcessedCount} avatars, success: {SuccessCount}, skipped: {SkippedCount}, error: {ErrorCount}",
                            processedCount, successCount, skipCount, errorCount);
                    }
                }
            }
            
            // 남은 문서 저장
            if (agentDocuments.Count > 0)
            {
                await SaveAgentDocuments(agentDocuments, stoppingToken);
            }
            
            _logger.Information(
                "Finished AgentRefreshInitializer. Processed {ProcessedCount} avatars, success: {SuccessCount}, skipped: {SkippedCount}, error: {ErrorCount}, elapsed: {TotalElapsedMinutes} minutes",
                processedCount, successCount, skipCount, errorCount, DateTime.UtcNow.Subtract(started).TotalMinutes);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Fatal error in AgentRefreshInitializer");
        }
    }
    
    private async Task<HashSet<string>> GetExistingAgentAddresses(CancellationToken stoppingToken)
    {
        try
        {
            var agentCollectionName = CollectionNames.GetCollectionName<AgentDocument>();
            var agentCollection = _dbService.GetCollection(agentCollectionName);
            
            // 에이전트 컬렉션에서 모든 문서의 ID만 가져오기 (ID는 주소의 헥스값)
            var filter = Builders<BsonDocument>.Filter.Empty;
            var projection = Builders<BsonDocument>.Projection.Include("_id");
            var options = new FindOptions<BsonDocument, BsonDocument>
            {
                Projection = projection
            };
            
            var addressSet = new HashSet<string>();
            using var cursor = await agentCollection.FindAsync(filter, options, stoppingToken);
            
            while (await cursor.MoveNextAsync(stoppingToken))
            {
                foreach (var doc in cursor.Current)
                {
                    if (doc.TryGetValue("_id", out var id) && id.IsString)
                    {
                        addressSet.Add(id.AsString);
                    }
                }
            }
            
            return addressSet;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting existing agent addresses");
            return new HashSet<string>();
        }
    }
    
    private async Task SaveAgentDocuments(List<MimirBsonDocument> documents, CancellationToken stoppingToken)
    {
        if (documents.Count == 0)
            return;
            
        try
        {
            await _dbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AgentDocument>(),
                documents,
                false,
                null,
                stoppingToken);
                
            _logger.Information("Saved batch of {Count} agent documents", documents.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving batch of {Count} agent documents", documents.Count);
        }
    }
} 