using Microsoft.Extensions.Logging;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using Libplanet.Crypto;

namespace Mimir.Scripts.Migrations;

public class UpdateTransactionDocumentMigration
{
    private readonly MongoDbService _mongoDbService;
    private readonly ILogger<UpdateTransactionDocumentMigration> _logger;
    private const string ProgressFileName = "migration_progress.json";

    public UpdateTransactionDocumentMigration(
        MongoDbService mongoDbService,
        ILogger<UpdateTransactionDocumentMigration> logger
    )
    {
        _mongoDbService = mongoDbService;
        _logger = logger;
    }

    public async Task<int> UpdateTransactionDocumentAsync(string databaseName)
    {
        _logger.LogInformation(
            "{DatabaseName} 데이터베이스 TransactionDocument 업데이트 시작",
            databaseName
        );

        var transactionCollection = _mongoDbService._transactionCollection;
        var blockCollection = _mongoDbService._blockCollection;

        _logger.LogDebug(
            "컬렉션 초기화 완료: Transaction={TransactionCollection}, Block={BlockCollection}",
            CollectionNames.GetCollectionName<TransactionDocument>(),
            CollectionNames.GetCollectionName<BlockDocument>()
        );

        var progress = await LoadProgressAsync(databaseName);
        var startBlockIndex = progress.LastProcessedBlockIndex - 1;

        _logger.LogInformation(
            "진행상황 로드 완료: 시작 BlockIndex={StartBlockIndex}",
            startBlockIndex
        );

        var filter = Builders<TransactionDocument>.Filter.Lte("BlockIndex", startBlockIndex);

        _logger.LogDebug("필터 조건: BlockIndex <= {StartBlockIndex}인 문서 검색", startBlockIndex);

        const int batchSize = 1000;
        var totalCount = 0;
        var batchCount = 0;
        var skip = 0;

        var sort = Builders<TransactionDocument>.Sort.Descending("BlockIndex");

        while (true)
        {
            try
            {
                var batchCursor = await transactionCollection.FindAsync(
                    filter,
                    new FindOptions<TransactionDocument> 
                    { 
                        Limit = batchSize, 
                        Skip = skip,
                        Sort = sort
                    }
                );

                var batch = await batchCursor.ToListAsync();

                if (batch.Count == 0)
                {
                    _logger.LogInformation(
                        "배치 처리 완료. 총 {TotalCount}개 문서 처리됨",
                        totalCount
                    );
                    break;
                }

                batchCount++;
                _logger.LogInformation(
                    "배치 {BatchNumber} 시작: {BatchSize}개 문서 처리 중... (최대 BlockIndex: {MaxBlockIndex})",
                    batchCount,
                    batch.Count,
                    batch.Max(d => d.BlockIndex)
                );

                var batchProcessedCount = 0;
                var currentBlockIndex = batch.Max(d => d.BlockIndex);

                foreach (var doc in batch)
                {
                    try
                    {
                        var updates = new List<UpdateDefinition<TransactionDocument>>();

                        await ProcessTransactionDocument(
                            doc,
                            transactionCollection,
                            blockCollection,
                            updates
                        );

                        _logger.LogDebug("업데이트 항목 수: {UpdateCount}", updates.Count);

                        updates.Add(
                            Builders<TransactionDocument>.Update.Set("Metadata.SchemaVersion", 4)
                        );

                        var updateFilter = Builders<TransactionDocument>.Filter.Eq("_id", doc.Id);
                        var combinedUpdate = Builders<TransactionDocument>.Update.Combine(updates);

                        transactionCollection.UpdateOne(updateFilter, combinedUpdate);
                        _logger.LogDebug(
                            "TransactionDocument 업데이트 완료: {DocumentId}, SchemaVersion: 4",
                            doc.Id
                        );
                        batchProcessedCount++;
                        totalCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "문서 업데이트 오류 (ID: {DocumentId})", doc.Id);
                    }
                }

                progress.LastProcessedBlockIndex = currentBlockIndex;
                progress.ProcessedCount = totalCount;
                await SaveProgressAsync(databaseName, progress);

                _logger.LogInformation(
                    "배치 {BatchNumber} 완료: {ProcessedCount}개 문서 처리됨, 현재 BlockIndex: {CurrentBlockIndex}",
                    batchCount,
                    batchProcessedCount,
                    currentBlockIndex
                );
                skip += batchSize;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        _logger.LogInformation(
            "{DatabaseName} 데이터베이스 TransactionDocument 업데이트 완료. {Count}개 문서 수정됨.",
            databaseName,
            totalCount
        );
        return totalCount;
    }

    private async Task<MigrationProgress> LoadProgressAsync(string databaseName)
    {
        var fileName = $"{databaseName}_{ProgressFileName}";
        
        if (File.Exists(fileName))
        {
            try
            {
                var json = await File.ReadAllTextAsync(fileName);
                var progress = JsonSerializer.Deserialize<MigrationProgress>(json);
                _logger.LogInformation(
                    "진행상황 파일 로드 완료: {FileName}, LastProcessedBlockIndex: {LastProcessedBlockIndex}",
                    fileName,
                    progress?.LastProcessedBlockIndex
                );
                return progress ?? new MigrationProgress();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "진행상황 파일 로드 실패: {FileName}", fileName);
            }
        }

        var newProgress = new MigrationProgress();
        await SaveProgressAsync(databaseName, newProgress);
        return newProgress;
    }

    private async Task SaveProgressAsync(string databaseName, MigrationProgress progress)
    {
        var fileName = $"{databaseName}_{ProgressFileName}";
        try
        {
            var json = JsonSerializer.Serialize(progress, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(fileName, json);
            _logger.LogDebug(
                "진행상황 저장 완료: {FileName}, LastProcessedBlockIndex: {LastProcessedBlockIndex}",
                fileName,
                progress.LastProcessedBlockIndex
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "진행상황 저장 실패: {FileName}", fileName);
        }
    }

    private async Task ProcessTransactionDocument(
        TransactionDocument doc,
        IMongoCollection<TransactionDocument> transactionCollection,
        IMongoCollection<BlockDocument> blockCollection,
        List<UpdateDefinition<TransactionDocument>> updates
    )
    {
        _logger.LogDebug("트랜잭션 문서 처리 시작: ID={DocumentId}", doc.Id);

        await ProcessExtractedActionValues(doc, updates);
        await UpdateBlockHashAndTimestamp(doc, blockCollection, updates);
        await UpdateSignerToHex(doc, updates);
    }

    private async Task ProcessExtractedActionValues(
        TransactionDocument doc,
        List<UpdateDefinition<TransactionDocument>> updates
    )
    {
        var actions = doc.Object.Actions;

        _logger.LogDebug("Actions 개수: {ActionCount}", actions.Count);

        if (actions.Count > 0)
        {
            var firstAction = actions[0];
            var rawAction = firstAction.Raw;

            _logger.LogDebug("Raw Action 길이: {RawActionLength}", rawAction?.Length ?? 0);

            if (!string.IsNullOrEmpty(rawAction))
            {
                try
                {
                    var extractedActionValues = ActionParser.ExtractActionValue(rawAction);
                    var extractedActionValuesDoc = new ExtractedActionValues(
                        extractedActionValues.TypeId,
                        extractedActionValues.AvatarAddress,
                        extractedActionValues.Sender,
                        extractedActionValues.Recipients,
                        extractedActionValues.FungibleAssetValues,
                        extractedActionValues.InvolvedAvatarAddresses,
                        extractedActionValues.InvolvedAddresses
                    );

                    updates.Add(
                        Builders<TransactionDocument>.Update.Set(
                            "extractedActionValues",
                            extractedActionValuesDoc
                        )
                    );
                    _logger.LogDebug(
                        "ExtractedActionValues 추가 완료: {DocumentId}, TypeId={TypeId}",
                        doc.Id,
                        extractedActionValues.TypeId
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "ExtractedActionValues 파싱 오류 (ID: {DocumentId}, RawAction: {RawAction})",
                        doc.Id,
                        rawAction
                    );
                }
            }
        }
    }

    private async Task UpdateBlockHashAndTimestamp(
        TransactionDocument doc,
        IMongoCollection<BlockDocument> blockCollection,
        List<UpdateDefinition<TransactionDocument>> updates
    )
    {
        var blockIndex = doc.BlockIndex;
        var currentBlockHash = doc.BlockHash;

        _logger.LogDebug(
            "BlockHashAndTimestamp 업데이트: BlockIndex={BlockIndex}, 현재 BlockHash={CurrentBlockHash}",
            blockIndex,
            currentBlockHash
        );

        if (string.IsNullOrEmpty(currentBlockHash))
        {
            var blockFilter = Builders<BlockDocument>.Filter.Eq("Object.Index", blockIndex);
            _logger.LogDebug("Block 검색 필터: Object.Index = {BlockIndex}", blockIndex);
            var blockCursor = await blockCollection.FindAsync(blockFilter);
            var blockDoc = await blockCursor.FirstOrDefaultAsync();

            if (blockDoc != null)
            {
                var blockHash = blockDoc.BlockHash;
                updates.Add(Builders<TransactionDocument>.Update.Set("BlockHash", blockHash));
                _logger.LogDebug(
                    "BlockHash 업데이트 완료: {DocumentId} -> {BlockHash}",
                    doc.Id,
                    blockHash
                );
            }
            else
            {
                _logger.LogWarning(
                    "Block을 찾을 수 없습니다. BlockIndex: {BlockIndex}",
                    blockIndex
                );
            }
        }
        else
        {
            _logger.LogDebug("BlockHash가 이미 존재함: {DocumentId}", doc.Id);
        }

        if (doc.BlockTimestamp == null)
        {
            var blockFilter = Builders<BlockDocument>.Filter.Eq("Object.Index", blockIndex);
            var blockCursor = await blockCollection.FindAsync(blockFilter);
            var blockDoc = await blockCursor.FirstOrDefaultAsync();

            if (blockDoc != null)
            {
                var blockObject = blockDoc.Object;
                if (blockObject.Timestamp != null)
                {
                    var blockTimestamp = blockObject.Timestamp.ToString();
                    updates.Add(
                        Builders<TransactionDocument>.Update.Set("BlockTimestamp", blockTimestamp)
                    );
                    _logger.LogDebug(
                        "BlockTimestamp 추가 완료: {DocumentId} -> {BlockTimestamp}",
                        doc.Id,
                        blockTimestamp
                    );
                }
                else
                {
                    _logger.LogDebug(
                        "Block에 Timestamp가 없음: BlockIndex={BlockIndex}",
                        blockIndex
                    );
                }
            }
            else
            {
                _logger.LogDebug(
                    "Block을 찾을 수 없어 BlockTimestamp 추가 불가: BlockIndex={BlockIndex}",
                    blockIndex
                );
            }
        }
        else
        {
            _logger.LogDebug("BlockTimestamp가 이미 존재함: {DocumentId}", doc.Id);
        }
    }

    private async Task UpdateSignerToHex(
        TransactionDocument doc,
        List<UpdateDefinition<TransactionDocument>> updates
    )
    {
        var transaction = doc.Object;
        var signer = transaction.Signer;

        _logger.LogDebug(
            "Signer 업데이트: ID={DocumentId}, 현재 Signer={CurrentSigner}",
            doc.Id,
            signer
        );

        try
        {
            var signerHex = signer.ToHex();
            updates.Add(
                Builders<TransactionDocument>.Update.Set("Object.Signer", signerHex)
            );
            _logger.LogDebug(
                "Signer ToHex 변환 완료: {DocumentId} -> {SignerHex}",
                doc.Id,
                signerHex
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Signer ToHex 변환 오류 (ID: {DocumentId}, Signer: {Signer})",
                doc.Id,
                signer
            );
        }
    }

    public async Task<int> ExecuteAsync()
    {
        _logger.LogInformation("MongoDB 연결 성공");

        var heimdallCount = await UpdateTransactionDocumentAsync("heimdall");
        var odinCount = await UpdateTransactionDocumentAsync("odin");

        _logger.LogInformation(
            "모든 업데이트 완료. 총 {TotalCount}개 문서 수정됨.",
            odinCount + heimdallCount
        );
        return odinCount + heimdallCount;
    }
}

public class MigrationProgress
{
    public long LastProcessedBlockIndex { get; set; } = long.MaxValue;
    public int ProcessedCount { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
