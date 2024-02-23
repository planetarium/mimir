using MongoDB.Bson;
using MongoDB.Driver;
using NineChroniclesUtilBackend.Store.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NineChroniclesUtilBackend.Store.Services;

public class MongoDbStore
{
    private readonly IMongoCollection<BsonDocument> _arenaCollection;
    private readonly IMongoCollection<BsonDocument> _avatarCollection;
    private BlockingCollection<ArenaData> _arenaDataQueue = new BlockingCollection<ArenaData>();
    private BlockingCollection<AvatarData> _avatarDataQueue = new BlockingCollection<AvatarData>();
    private readonly int _batchSize = 100;

    public MongoDbStore(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);

        _arenaCollection = database.GetCollection<BsonDocument>("arena");
        _avatarCollection = database.GetCollection<BsonDocument>("avatars");

        Task.Run(() => ProcessArenaDataAsync());

        Task.Run(() => ProcessAvatarDataAsync());
    }

    public void AddArenaData(ArenaData arenaData)
    {
        _arenaDataQueue.Add(arenaData);
    }

    public void AddAvatarData(AvatarData avatarData)
    {
        _avatarDataQueue.Add(avatarData);
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
        foreach (var arenaData in arenaDatas)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("avatarAddress", arenaData.AvatarAddress.ToHex());
            var bsonDocument = BsonDocument.Parse(arenaData.ToJson());
            var upsertOne = new ReplaceOneModel<BsonDocument>(filter, bsonDocument) { IsUpsert = true };
            bulkOps.Add(upsertOne);
        }
        if (bulkOps.Count > 0)
        {
            await _arenaCollection.BulkWriteAsync(bulkOps);
        }
    }

    private async Task BulkUpsertAvatarDataAsync(List<AvatarData> avatarDatas)
    {
        var bulkOps = new List<WriteModel<BsonDocument>>();
        foreach (var avatarData in avatarDatas)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("avatar.address", avatarData.Avatar.address.ToHex());
            var bsonDocument = BsonDocument.Parse(avatarData.ToJson());
            var upsertOne = new ReplaceOneModel<BsonDocument>(filter, bsonDocument) { IsUpsert = true };
            bulkOps.Add(upsertOne);
        }
        if (bulkOps.Count > 0)
        {
            await _avatarCollection.BulkWriteAsync(bulkOps);
        }
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
    }
}