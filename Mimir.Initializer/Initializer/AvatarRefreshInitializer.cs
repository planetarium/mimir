using Lib9c.Models.Extensions;
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
using Nekoyume;
using Serilog;
using ILogger = Serilog.ILogger;
using Mimir.MongoDB;

namespace Mimir.Initializer.Initializer;

public class AvatarRefreshInitializer : IExecutor
{
    private readonly IMongoDbService _dbService;
    private readonly IStateService _stateService;
    private readonly StateGetter _stateGetter;
    private readonly ILogger _logger;
    private readonly bool _shouldRun;
    private readonly AvatarStateDocumentConverter _avatarConverter;
    
    public AvatarRefreshInitializer(
        IOptions<Configuration> configuration,
        IMongoDbService dbService,
        IStateService stateService)
    {
        _dbService = dbService;
        _stateService = stateService;
        _stateGetter = stateService.At(configuration);
        _shouldRun = configuration.Value.RunOptions.HasFlag(RunOptions.AvatarRefreshInitializer);
        _logger = Log.ForContext<AvatarRefreshInitializer>();
        _avatarConverter = new AvatarStateDocumentConverter();
    }
    
    public bool ShouldRun() => _shouldRun;
    
    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        _logger.Information("Starting AvatarRefreshInitializer");
        
        try
        {
            // 이미 DB에 있는 아바타 주소 목록 가져오기
            var existingAvatarAddresses = await GetExistingAvatarAddresses(stoppingToken);
            _logger.Information("Found {Count} existing avatar documents in database", existingAvatarAddresses.Count);
            
            // 에이전트 컬렉션에서 모든 문서 가져오기
            var agentCollectionName = CollectionNames.GetCollectionName<AgentDocument>();
            var agentCollection = _dbService.GetCollection(agentCollectionName);
            
            // 배치 크기 설정
            const int batchSize = 100;
            var filter = Builders<BsonDocument>.Filter.Empty;
            var options = new FindOptions<BsonDocument>
            {
                BatchSize = batchSize
            };
            
            using var cursor = await agentCollection.FindAsync(filter, options, stoppingToken);
            var processedCount = 0;
            var successCount = 0;
            var skipCount = 0;
            var errorCount = 0;
            
            // 아바타 문서를 저장할 리스트
            var avatarDocuments = new List<MimirBsonDocument>();
            var processedAvatarAddresses = new HashSet<string>(existingAvatarAddresses);
            
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
                        // BsonDocument를 AgentDocument로 변환
                        AgentDocument? agentDoc = null;
                        try
                        {
                            agentDoc = BsonSerializer.Deserialize<AgentDocument>(bsonDoc);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning(ex, "Failed to deserialize BsonDocument to AgentDocument");
                            errorCount++;
                            continue;
                        }
                        
                        if (agentDoc == null || agentDoc.Object == null)
                        {
                            _logger.Warning("AgentDocument or its Object is null");
                            errorCount++;
                            continue;
                        }
                        
                        var agentState = agentDoc.Object;
                        var agentAddress = agentDoc.Address;
                        
                        // 에이전트에 연결된 아바타 주소 확인
                        foreach (var avatarAddressEntry in agentState.AvatarAddresses)
                        {
                            var avatarAddress = avatarAddressEntry.Value;
                            
                            // 이미 처리한 아바타 주소는 건너뛰기
                            if (processedAvatarAddresses.Contains(avatarAddress.ToHex()))
                            {
                                skipCount++;
                                continue;
                            }
                            
                            // 아바타 상태 가져오기
                            var avatarState = await _stateGetter.GetStateWithLegacyAccount(
                                avatarAddress,
                                Addresses.Avatar,
                                stoppingToken);
                            
                            if (avatarState != null)
                            {
                                int? armorId = null;
                                int? portraitId = null;
                                
                                // 추가 정보 가져오기 (inventory)
                                try
                                {
                                    var inventoryState = await _stateGetter.GetInventoryState(avatarAddress, stoppingToken);
                                    
                                    // 확장 메서드를 사용하여 armorId와 portraitId 가져오기
                                    if (inventoryState != null)
                                    {
                                        armorId = inventoryState.GetArmorId();
                                        portraitId = inventoryState.GetPortraitId();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Warning(ex, "Failed to get inventory data for avatar {AvatarAddress}", avatarAddress.ToHex());
                                }
                                
                                var blockIndex = await _stateService.GetLatestIndex(stoppingToken);
                                
                                // AddressStatePair 생성
                                var pair = new AddressStatePair
                                {
                                    BlockIndex = blockIndex,
                                    Address = avatarAddress,
                                    RawState = avatarState,
                                    AdditionalData = new Dictionary<string, object>
                                    {
                                        { "armorId", armorId },
                                        { "portraitId", portraitId },
                                    },
                                };
                                
                                // 아바타 문서 생성
                                var newAvatarDoc = _avatarConverter.ConvertToDocument(pair);
                                avatarDocuments.Add(newAvatarDoc);
                                processedAvatarAddresses.Add(avatarAddress.ToHex());
                                successCount++;
                                
                                // 일정 개수마다 배치 저장
                                if (avatarDocuments.Count >= batchSize)
                                {
                                    await SaveAvatarDocuments(avatarDocuments, stoppingToken);
                                    avatarDocuments.Clear();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error processing agent document");
                        errorCount++;
                    }
                    
                    // 로그 표시
                    if (processedCount % 100 == 0)
                    {
                        _logger.Information(
                            "Processed {ProcessedCount} agents, success: {SuccessCount}, skipped: {SkippedCount}, error: {ErrorCount}",
                            processedCount, successCount, skipCount, errorCount);
                    }
                }
            }
            
            // 남은 문서 저장
            if (avatarDocuments.Count > 0)
            {
                await SaveAvatarDocuments(avatarDocuments, stoppingToken);
            }
            
            _logger.Information(
                "Finished AvatarRefreshInitializer. Processed {ProcessedCount} agents, success: {SuccessCount}, skipped: {SkippedCount}, error: {ErrorCount}, elapsed: {TotalElapsedMinutes} minutes",
                processedCount, successCount, skipCount, errorCount, DateTime.UtcNow.Subtract(started).TotalMinutes);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Fatal error in AvatarRefreshInitializer");
        }
    }
    
    private async Task<HashSet<string>> GetExistingAvatarAddresses(CancellationToken stoppingToken)
    {
        try
        {
            var avatarCollectionName = CollectionNames.GetCollectionName<AvatarDocument>();
            var avatarCollection = _dbService.GetCollection(avatarCollectionName);
            
            // 아바타 컬렉션에서 모든 문서의 ID만 가져오기 (ID는 주소의 헥스값)
            var filter = Builders<BsonDocument>.Filter.Empty;
            var projection = Builders<BsonDocument>.Projection.Include("_id");
            var options = new FindOptions<BsonDocument, BsonDocument>
            {
                Projection = projection
            };
            
            var addressSet = new HashSet<string>();
            using var cursor = await avatarCollection.FindAsync(filter, options, stoppingToken);
            
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
            _logger.Error(ex, "Error getting existing avatar addresses");
            return new HashSet<string>();
        }
    }
    
    private async Task SaveAvatarDocuments(List<MimirBsonDocument> documents, CancellationToken stoppingToken)
    {
        if (documents.Count == 0)
            return;
            
        try
        {
            await _dbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AvatarDocument>(),
                documents,
                false,
                null,
                stoppingToken);
                
            _logger.Information("Saved batch of {Count} avatar documents", documents.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving batch of {Count} avatar documents", documents.Count);
        }
    }
} 