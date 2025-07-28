using Microsoft.Extensions.Logging;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Scripts.Migrations;

public class UpdateLastStageClearedIdMigration
{
    private readonly Mimir.Worker.Services.MongoDbService _mongoDbService;
    private readonly ILogger<UpdateLastStageClearedIdMigration> _logger;

    public UpdateLastStageClearedIdMigration(
        Mimir.Worker.Services.MongoDbService mongoDbService,
        ILogger<UpdateLastStageClearedIdMigration> logger
    )
    {
        _mongoDbService = mongoDbService;
        _logger = logger;
    }

    public async Task<int> UpdateLastStageClearedIdAsync(string databaseName)
    {
        _logger.LogInformation("{DatabaseName} 데이터베이스 업데이트 시작", databaseName);

        var collection = _mongoDbService.GetCollection(
            CollectionNames.GetCollectionName<WorldInformationDocument>()
        );

        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Exists("Object.WorldDictionary"),
            Builders<BsonDocument>.Filter.Eq("Metadata.SchemaVersion", 1)
        );

        var cursor = await collection.FindAsync(filter);
        var count = 0;

        if (cursor == null)
        {
            _logger.LogWarning("커서가 null입니다. 데이터베이스: {DatabaseName}", databaseName);
            return count;
        }

        while (await cursor.MoveNextAsync())
        {
            if (cursor.Current == null)
            {
                _logger.LogWarning(
                    "커서의 Current가 null입니다. 데이터베이스: {DatabaseName}",
                    databaseName
                );
                continue;
            }

            foreach (var doc in cursor.Current)
            {
                var worldDictionary = doc.GetValue("Object", new BsonDocument())
                    .AsBsonDocument.GetValue("WorldDictionary", new BsonDocument())
                    .AsBsonDocument;

                if (worldDictionary == null || worldDictionary.ElementCount == 0)
                {
                    _logger.LogInformation(
                        "WorldDictionary가 존재하지 않음, 건너뜀: {DocumentId}",
                        doc["_id"]
                    );
                    continue;
                }

                var worldKeys = worldDictionary
                    .Names.Where(key => int.TryParse(key, out var num) && num < 10000)
                    .Select(key => int.Parse(key))
                    .ToList();

                if (!worldKeys.Any())
                {
                    _logger.LogInformation(
                        "유효한 월드 키가 없음, 건너뜀: {DocumentId}",
                        doc["_id"]
                    );
                    continue;
                }

                var highestStageClearedId = -1;
                var highestWorldId = -1;

                foreach (var key in worldKeys)
                {
                    var world = worldDictionary
                        .GetValue(key.ToString(), new BsonDocument())
                        .AsBsonDocument;
                    if (world.Contains("StageClearedId"))
                    {
                        var stageClearedId = world["StageClearedId"].AsInt32;
                        if (stageClearedId != -1 && stageClearedId > highestStageClearedId)
                        {
                            highestStageClearedId = stageClearedId;
                            highestWorldId = key;
                        }
                    }
                }

                if (highestStageClearedId == -1)
                {
                    _logger.LogInformation(
                        "StageClearedId가 -1이 아닌 월드가 없음, 건너뜀: {DocumentId}",
                        doc["_id"]
                    );
                    continue;
                }

                _logger.LogInformation(
                    "문서 ID: {DocumentId}, 월드 ID: {WorldId}, 가장 높은 StageClearedId: {StageClearedId}",
                    doc["_id"],
                    highestWorldId,
                    highestStageClearedId
                );

                try
                {
                    var updateFilter = Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]);
                    var update = Builders<BsonDocument>
                        .Update.Set("LastStageClearedId", highestStageClearedId)
                        .Set("Metadata.SchemaVersion", 2);

                    collection.UpdateOne(updateFilter, update);
                    _logger.LogInformation(
                        "LastStageClearedId 업데이트 완료: {StageClearedId}, SchemaVersion: 2",
                        highestStageClearedId
                    );
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "문서 업데이트 오류 (ID: {DocumentId})", doc["_id"]);
                }
            }
        }

        _logger.LogInformation(
            "{DatabaseName} 데이터베이스 업데이트 완료. {Count}개 문서 수정됨.",
            databaseName,
            count
        );
        return count;
    }

    public async Task<int> ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("MongoDB 연결 성공");

            var odinCount = await UpdateLastStageClearedIdAsync("odin");
            var heimdallCount = await UpdateLastStageClearedIdAsync("heimdall");

            _logger.LogInformation(
                "모든 업데이트 완료. 총 {TotalCount}개 문서 수정됨.",
                odinCount + heimdallCount
            );
            return odinCount + heimdallCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "업데이트 과정에서 오류 발생");
            throw;
        }
    }
}

// 테스트용 코드 예시 (xUnit 스타일)
#if TEST
using Xunit;

public class UpdateLastStageClearedIdMigrationTest
{
    [Fact]
    public void 생성_테스트()
    {
        var mongo = new Mimir.Worker.Services.MongoDbService("mongodb://localhost", Mimir.Worker.Constants.PlanetType.Odin, null);
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<UpdateLastStageClearedIdMigration>();
        var migration = new UpdateLastStageClearedIdMigration(mongo, logger);
        Assert.NotNull(migration);
    }
}
#endif
