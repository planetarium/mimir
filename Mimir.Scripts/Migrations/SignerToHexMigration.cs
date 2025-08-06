using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using Libplanet.Crypto;

namespace Mimir.Scripts.Migrations;

public class SignerToHexMigration
{
    private readonly IMongoDbService _mongoDbService;
    private readonly ILogger<SignerToHexMigration> _logger;
    private readonly Configuration _configuration;
    private const string ProgressFileName = "signer_tohex_migration_progress.json";

    public SignerToHexMigration(
        IMongoDbService mongoDbService,
        ILogger<SignerToHexMigration> logger,
        IOptions<Configuration> configuration
    )
    {
        _mongoDbService = mongoDbService;
        _logger = logger;
        _configuration = configuration.Value;
    }

    public async Task<int> UpdateSignerToHexAsync(string databaseName)
    {
        _logger.LogInformation(
            "Signer ToHex 마이그레이션 시작: Database={DatabaseName}",
            databaseName
        );

        var transactionCollection = _mongoDbService.GetCollection<TransactionDocument>(
            CollectionNames.GetCollectionName<TransactionDocument>()
        );

        var progress = await LoadProgressAsync(databaseName);
        var totalProcessed = 0;
        var batchSize = 1000;

        _logger.LogInformation(
            "마지막 처리된 블록 인덱스: {LastProcessedBlockIndex}",
            progress.LastProcessedBlockIndex
        );

        while (true)
        {
            var blockIndexFilter = Builders<TransactionDocument>.Filter.Lt("BlockIndex", progress.LastProcessedBlockIndex);
            var sort = Builders<TransactionDocument>.Sort.Descending("BlockIndex");

            var transactionCursor = await transactionCollection
                .Find(blockIndexFilter)
                .Sort(sort)
                .Limit(batchSize)
                .ToCursorAsync();

            var transactions = await transactionCursor.ToListAsync();

            if (transactions.Count == 0)
            {
                _logger.LogInformation("더 이상 처리할 트랜잭션이 없습니다.");
                break;
            }

            _logger.LogInformation(
                "배치 처리 중: {Count}개 트랜잭션, 블록 인덱스 범위: {MaxBlockIndex} ~ {MinBlockIndex}",
                transactions.Count,
                transactions.Max(t => t.BlockIndex),
                transactions.Min(t => t.BlockIndex)
            );

            var updates = new List<UpdateDefinition<TransactionDocument>>();
            var processedInBatch = 0;

            foreach (var doc in transactions)
            {
                UpdateSignerToHex(doc, updates);
                processedInBatch++;
            }

            if (updates.Count > 0)
            {
                var bulkWrites = new List<WriteModel<TransactionDocument>>();
                var updateIndex = 0;

                foreach (var doc in transactions)
                {
                    if (updateIndex < updates.Count)
                    {
                        var docFilter = Builders<TransactionDocument>.Filter.Eq("_id", doc.Id);
                        var updateOne = new UpdateOneModel<TransactionDocument>(docFilter, updates[updateIndex]);
                        bulkWrites.Add(updateOne);
                        updateIndex++;
                    }
                }

                if (bulkWrites.Count > 0)
                {
                    await transactionCollection.BulkWriteAsync(bulkWrites);
                    _logger.LogInformation(
                        "배치 업데이트 완료: {UpdateCount}개 문서 수정됨",
                        bulkWrites.Count
                    );
                }
            }

            totalProcessed += processedInBatch;
            progress.ProcessedCount += processedInBatch;
            progress.LastProcessedBlockIndex = transactions.Min(t => t.BlockIndex);
            progress.LastUpdated = DateTime.UtcNow;

            await SaveProgressAsync(databaseName, progress);

            _logger.LogInformation(
                "진행상황: 총 {TotalProcessed}개 처리됨, 마지막 블록 인덱스: {LastProcessedBlockIndex}",
                totalProcessed,
                progress.LastProcessedBlockIndex
            );
        }

        _logger.LogInformation(
            "Signer ToHex 마이그레이션 완료: Database={DatabaseName}, 총 {TotalProcessed}개 처리됨",
            databaseName,
            totalProcessed
        );

        return totalProcessed;
    }

    private async Task<SignerMigrationProgress> LoadProgressAsync(string databaseName)
    {
        var fileName = $"{_configuration.PlanetType.ToString().ToLowerInvariant()}_{databaseName}_{ProgressFileName}";
        try
        {
            if (File.Exists(fileName))
            {
                var json = await File.ReadAllTextAsync(fileName);
                var progress = JsonSerializer.Deserialize<SignerMigrationProgress>(json);
                _logger.LogInformation(
                    "진행상황 로드 완료: {FileName}, LastProcessedBlockIndex: {LastProcessedBlockIndex}",
                    fileName,
                    progress?.LastProcessedBlockIndex
                );
                return progress ?? new SignerMigrationProgress();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "진행상황 로드 실패: {FileName}", fileName);
        }

        var newProgress = new SignerMigrationProgress();
        _logger.LogInformation(
            "새로운 진행상황 생성: LastProcessedBlockIndex={LastProcessedBlockIndex}",
            newProgress.LastProcessedBlockIndex
        );
        return newProgress;
    }

    private async Task SaveProgressAsync(string databaseName, SignerMigrationProgress progress)
    {
        var fileName = $"{_configuration.PlanetType.ToString().ToLowerInvariant()}_{databaseName}_{ProgressFileName}";
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

    private void UpdateSignerToHex(
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

        var heimdallCount = await UpdateSignerToHexAsync("heimdall");
        var odinCount = await UpdateSignerToHexAsync("odin");

        _logger.LogInformation(
            "모든 Signer ToHex 업데이트 완료. 총 {TotalCount}개 문서 수정됨.",
            odinCount + heimdallCount
        );
        return odinCount + heimdallCount;
    }
}

public class SignerMigrationProgress
{
    public long LastProcessedBlockIndex { get; set; } = long.MaxValue;
    public int ProcessedCount { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}