using Bencodex.Types;
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

public class AdventureCpRefreshInitializer : IExecutor
{
    private readonly IMongoDbService _dbService;
    private readonly IStateService _stateService;
    private readonly StateGetter _stateGetter;
    private readonly ILogger _logger;
    private readonly bool _shouldRun;
    private readonly AdventureCpStateDocumentConverter _adventureCpConverter;
    
    public AdventureCpRefreshInitializer(
        IOptions<Configuration> configuration,
        IMongoDbService dbService,
        IStateService stateService)
    {
        _dbService = dbService;
        _stateService = stateService;
        _stateGetter = stateService.At(configuration);
        _shouldRun = configuration.Value.RunOptions.HasFlag(RunOptions.AdventureCpRefreshInitializer);
        _logger = Log.ForContext<AdventureCpRefreshInitializer>();
        _adventureCpConverter = new AdventureCpStateDocumentConverter();
    }
    
    public bool ShouldRun() => _shouldRun;
    
    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        _logger.Information("Starting AdventureCpRefreshInitializer");
        
        try
        {
            // 이미 DB에 있는 어드벤처 CP 주소 목록 가져오기
            var existingCpAddresses = await GetExistingCpAddresses(stoppingToken);
            _logger.Information("Found {Count} existing adventure CP documents in database", existingCpAddresses.Count);
            
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
            
            // 어드벤처 CP 문서를 저장할 리스트
            var cpDocuments = new List<MimirBsonDocument>();
            var processedCpAddresses = new HashSet<string>(existingCpAddresses);
            
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
                        
                        var avatarAddress = avatarDoc.Object.Address;
                        
                        // 이미 처리한 어드벤처 CP 주소는 건너뛰기 (아바타 주소와 어드벤처 CP 주소는 동일)
                        if (processedCpAddresses.Contains(avatarAddress.ToHex()))
                        {
                            skipCount++;
                            continue;
                        }
                        
                        // 어드벤처 CP 상태 가져오기
                        var cpState = await _stateService.GetState(
                            avatarAddress,
                            Addresses.AdventureCp,
                            stoppingToken);
                        
                        if (cpState != null)
                        {
                            var blockIndex = await _stateService.GetLatestIndex(stoppingToken);
                            
                            // Dictionary 형식인 경우 List 형식으로 변환
                            IValue processedCpState = cpState;

                            // AddressStatePair 생성
                            var pair = new AddressStatePair
                            {
                                BlockIndex = blockIndex,
                                Address = avatarAddress,
                                RawState = processedCpState
                            };
                            
                            try
                            {
                                // 어드벤처 CP 문서 생성 - 커스텀 컨버터 사용
                                var cpDoc = _adventureCpConverter.ConvertToDocument(pair);
                                cpDocuments.Add(cpDoc);
                                processedCpAddresses.Add(avatarAddress.ToHex());
                                successCount++;
                                
                                // 일정 개수마다 배치 저장
                                if (cpDocuments.Count >= batchSize)
                                {
                                    await SaveCpDocuments(cpDocuments, stoppingToken);
                                    cpDocuments.Clear();
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "Error creating adventure CP document for avatar {AvatarAddress}: {ErrorMessage}", 
                                    avatarAddress.ToHex(), ex.Message);
                                
                                // 디버깅을 위해 상태 정보 출력
                                if (processedCpState is List rawList)
                                {
                                    _logger.Debug("CP state type: {StateType}, First value type: {FirstValueType}", 
                                        processedCpState.GetType().Name, 
                                        rawList.Count > 0 ? rawList[0].GetType().Name : "N/A");
                                }
                                
                                errorCount++;
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
            if (cpDocuments.Count > 0)
            {
                await SaveCpDocuments(cpDocuments, stoppingToken);
            }
            
            _logger.Information(
                "Finished AdventureCpRefreshInitializer. Processed {ProcessedCount} avatars, success: {SuccessCount}, skipped: {SkippedCount}, error: {ErrorCount}, elapsed: {TotalElapsedMinutes} minutes",
                processedCount, successCount, skipCount, errorCount, DateTime.UtcNow.Subtract(started).TotalMinutes);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Fatal error in AdventureCpRefreshInitializer");
        }
    }
    
    private async Task<HashSet<string>> GetExistingCpAddresses(CancellationToken stoppingToken)
    {
        try
        {
            var cpCollectionName = CollectionNames.GetCollectionName<AdventureCpDocument>();
            var cpCollection = _dbService.GetCollection(cpCollectionName);
            
            // CP 컬렉션에서 모든 문서의 ID만 가져오기 (ID는 주소의 헥스값)
            var filter = Builders<BsonDocument>.Filter.Empty;
            var projection = Builders<BsonDocument>.Projection.Include("_id");
            var options = new FindOptions<BsonDocument, BsonDocument>
            {
                Projection = projection
            };
            
            var addressSet = new HashSet<string>();
            using var cursor = await cpCollection.FindAsync(filter, options, stoppingToken);
            
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
            _logger.Error(ex, "Error getting existing adventure CP addresses");
            return new HashSet<string>();
        }
    }
    
    private async Task SaveCpDocuments(List<MimirBsonDocument> documents, CancellationToken stoppingToken)
    {
        if (documents.Count == 0)
            return;
            
        try
        {
            await _dbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AdventureCpDocument>(),
                documents,
                false,
                null,
                stoppingToken);
                
            _logger.Information("Saved batch of {Count} adventure CP documents", documents.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving batch of {Count} adventure CP documents", documents.Count);
        }
    }
} 