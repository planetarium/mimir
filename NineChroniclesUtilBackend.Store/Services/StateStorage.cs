using Libplanet.Crypto;
using MongoDB.Bson;
using MongoDB.Driver;
using NineChroniclesUtilBackend.Store.Models;

namespace NineChroniclesUtilBackend.Store.Services;

public class StateStorage : IStateStorage
{
    private readonly ILogger<StateStorage> _logger;
    private readonly IMongoDatabase _database;

    private IMongoCollection<BsonDocument> ArenaCollection => _database.GetCollection<BsonDocument>("arena");
    private IMongoCollection<BsonDocument> AvatarCollection => _database.GetCollection<BsonDocument>("avatars");

    public StateStorage(ILogger<StateStorage> logger, IMongoDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    public Task AddArenaData(ArenaData arenaData)
    {
        return BulkUpsertArenaDataAsync(new List<ArenaData> { arenaData });
    }
    
    public Task AddArenaData(List<ArenaData> arenaData)
    {
        return BulkUpsertArenaDataAsync(arenaData);
    }

    public Task AddAvatarData(AvatarData avatarData)
    {
        return BulkUpsertAvatarDataAsync(new List<AvatarData> { avatarData });
    }
    
    public Task AddAvatarData(List<AvatarData> avatarData)
    {
        return BulkUpsertAvatarDataAsync(avatarData);
    }
    
    public async Task LinkAvatarWithArenaAsync(Address address)
    {
        var avatarFilter = Builders<BsonDocument>.Filter.Eq("Avatar.address", address.ToHex());
        var avatar = await AvatarCollection.Find(avatarFilter).FirstOrDefaultAsync();
        if (avatar != null)
        {
            var objectId = avatar["_id"].AsObjectId;
            var arenaFilter = Builders<BsonDocument>.Filter.Eq("AvatarAddress", address.ToHex());
            var update = Builders<BsonDocument>.Update.Set("AvatarObjectId", objectId);
            var updateModel = new UpdateOneModel<BsonDocument>(arenaFilter, update) { IsUpsert = false };
            await ArenaCollection.BulkWriteAsync(new List<WriteModel<BsonDocument>> { updateModel });
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
                using var session = await _database.Client.StartSessionAsync();
                await ArenaCollection.BulkWriteAsync(bulkOps);
            }

            _logger.LogInformation($"Stored {bulkOps.Count} arena data");
        }
        catch(Exception ex)
        {
            _logger.LogError($"An error occurred during BulkUpsertArenaDataAsync: {ex.Message}");
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
                await AvatarCollection.BulkWriteAsync(bulkOps);
            }

            _logger.LogInformation($"Stored {bulkOps.Count} avatar data");
        }
        catch(Exception ex)
        {
            _logger.LogError($"An error occurred during BulkUpsertAvatarDataAsync: {ex.Message}");
        }
    }
}