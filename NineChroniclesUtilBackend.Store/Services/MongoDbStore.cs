using Libplanet.Crypto;
using MongoDB.Bson;
using MongoDB.Driver;
using NineChroniclesUtilBackend.Store.Models;
using System.Collections.Concurrent;

namespace NineChroniclesUtilBackend.Store.Services;

public class MongoDbStore
{
    private readonly ILogger<MongoDbStore> _logger;
    private readonly IMongoCollection<BsonDocument> _arenaCollection;
    private readonly IMongoCollection<BsonDocument> _avatarCollection;
    private BlockingCollection<ArenaData> _arenaDataQueue = new BlockingCollection<ArenaData>();
    private BlockingCollection<AvatarData> _avatarDataQueue = new BlockingCollection<AvatarData>();
    private List<Address> _avatarAddresses = new List<Address>();
    private readonly int _batchSize = 100;

    public readonly StoreResult Result = new StoreResult();

    public MongoDbStore(ILogger<MongoDbStore> logger, string connectionString, string databaseName)
    {
        _logger = logger;

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);

        _arenaCollection = database.GetCollection<BsonDocument>("arena");
        _avatarCollection = database.GetCollection<BsonDocument>("avatars");

        Task.Run(() => ProcessArenaDataAsync());
        Task.Run(() => ProcessAvatarDataAsync());

        Result.StartTime = DateTime.UtcNow;
    }

    public void AddArenaData(ArenaData arenaData)
    {
        _arenaDataQueue.Add(arenaData);

        Result.StoreArenaRequestCount += 1;
    }

    public void AddAvatarData(AvatarData avatarData)
    {
        _avatarDataQueue.Add(avatarData);
        _avatarAddresses.Add(avatarData.Avatar.address);

        Result.StoreAvatarRequestCount += 1;
    }

    private async Task ProcessArenaDataAsync()
    {
        List<ArenaData> batch = new List<ArenaData>();
        foreach (var arenaData in _arenaDataQueue.GetConsumingEnumerable())
        {
            batch.Add(arenaData);
            if (batch.Count >= _batchSize)
            {
                await BulkUpsertArenaDataAsync(batch);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await BulkUpsertArenaDataAsync(batch);
        }
    }

    private async Task ProcessAvatarDataAsync()
    {
        List<AvatarData> batch = new List<AvatarData>();
        foreach (var avatarData in _avatarDataQueue.GetConsumingEnumerable())
        {
            batch.Add(avatarData);
            if (batch.Count >= _batchSize)
            {
                await BulkUpsertAvatarDataAsync(batch);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await BulkUpsertAvatarDataAsync(batch);
        }
    }

    private async Task BulkUpsertArenaDataAsync(List<ArenaData> arenaDatas)
    {
        var bulkOps = new List<WriteModel<BsonDocument>>();

        try
        {
            foreach (var arenaData in arenaDatas)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("AvatarAddress", arenaData.AvatarAddress.ToHex());
                var bsonDocument = BsonDocument.Parse(arenaData.ToJson());
                var upsertOne = new ReplaceOneModel<BsonDocument>(filter, bsonDocument) { IsUpsert = true };
                bulkOps.Add(upsertOne);
            }
            if (bulkOps.Count > 0)
            {
                await _arenaCollection.BulkWriteAsync(bulkOps);
            }

            Result.ArenaStoredCount += bulkOps.Count;
            _logger.LogInformation($"Stored {bulkOps.Count} arena data");
        }
        catch(Exception ex)
        {
            _logger.LogError($"An error occurred during BulkUpsertArenaDataAsync: {ex.Message}");
            Result.FailedArenaAddresses.AddRange(arenaDatas.Select(d => d.AvatarAddress) );
        }
    }

    private async Task BulkUpsertAvatarDataAsync(List<AvatarData> avatarDatas)
    {
        var bulkOps = new List<WriteModel<BsonDocument>>();

        try
        {
            foreach (var avatarData in avatarDatas)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("Avatar.address", avatarData.Avatar.address.ToHex());
                var bsonDocument = BsonDocument.Parse(avatarData.ToJson());
                var upsertOne = new ReplaceOneModel<BsonDocument>(filter, bsonDocument) { IsUpsert = true };
                bulkOps.Add(upsertOne);
            }
            if (bulkOps.Count > 0)
            {
                await _avatarCollection.BulkWriteAsync(bulkOps);
            }

            Result.AvatarStoredCount += bulkOps.Count;
            _logger.LogInformation($"Stored {bulkOps.Count} avatar data");
        }
        catch(Exception ex)
        {
            _logger.LogError($"An error occurred during BulkUpsertAvatarDataAsync: {ex.Message}");
            Result.FailedAvatarAddresses.AddRange(avatarDatas.Select(d => d.Avatar.address) );
        }
    }

    public async Task LinkAvatarsToArenaAsync()
    {
        const int batchSize = 500;

        _logger.LogInformation($"Start Link Avatars");

        for (int i = 0; i < _avatarAddresses.Count; i += batchSize)
        {
            var batchAddresses = _avatarAddresses.Skip(i).Take(batchSize).ToList();
            var bulkOps = new List<WriteModel<BsonDocument>>();

            foreach (var address in batchAddresses)
            {
                var avatarFilter = Builders<BsonDocument>.Filter.Eq("Avatar.address", address.ToHex());
                var avatar = await _avatarCollection.Find(avatarFilter).FirstOrDefaultAsync();
                if (avatar != null)
                {
                    var objectId = avatar["_id"].AsObjectId;
                    var arenaFilter = Builders<BsonDocument>.Filter.Eq("AvatarAddress", address.ToHex());
                    var update = Builders<BsonDocument>.Update.Set("AvatarObjectId", objectId);
                    var updateModel = new UpdateOneModel<BsonDocument>(arenaFilter, update) { IsUpsert = false };
                    bulkOps.Add(updateModel);
                }
            }

            if (bulkOps.Count > 0)
            {
                await _arenaCollection.BulkWriteAsync(bulkOps);
            }
        }

        _logger.LogInformation($"Finish Link Avatars");
    }

    public async Task FlushAsync()
    {
        _arenaDataQueue.CompleteAdding();
        _avatarDataQueue.CompleteAdding();

        var remainingArenaDatas = new List<ArenaData>();
        while (_arenaDataQueue.TryTake(out var arenaData))
        {
            remainingArenaDatas.Add(arenaData);
        }
        if (remainingArenaDatas.Any())
        {
            await BulkUpsertArenaDataAsync(remainingArenaDatas);
        }

        var remainingAvatarDatas = new List<AvatarData>();
        while (_avatarDataQueue.TryTake(out var avatarData))
        {
            remainingAvatarDatas.Add(avatarData);
        }
        if (remainingAvatarDatas.Any())
        {
            await BulkUpsertAvatarDataAsync(remainingAvatarDatas);
        }

        _logger.LogInformation($"Finish Flushing");
    }
}