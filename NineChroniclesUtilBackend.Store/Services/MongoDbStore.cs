using MongoDB.Bson;
using MongoDB.Driver;
using NineChroniclesUtilBackend.Store.Models;

namespace NineChroniclesUtilBackend.Store.Services;

public class MongoDbStore
{
    private readonly IMongoCollection<BsonDocument> _arenaCollection;
    private readonly IMongoCollection<BsonDocument> _avatarCollection;

    public MongoDbStore(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);

        _arenaCollection = database.GetCollection<BsonDocument>("arena");
        _avatarCollection = database.GetCollection<BsonDocument>("avatars");
    }

    public async Task SaveArenaDataAsync(ArenaData arenaData)
    {
        var jsonString = arenaData.ToJson();
        var bsonDocument = BsonDocument.Parse(jsonString);
        await _arenaCollection.InsertOneAsync(bsonDocument);
    }

    public async Task SaveAvatarDataAsync(AvatarData avatarData)
    {
        var jsonString = avatarData.ToJson();
        var bsonDocument = BsonDocument.Parse(jsonString);
        await _avatarCollection.InsertOneAsync(bsonDocument);
    }
}
