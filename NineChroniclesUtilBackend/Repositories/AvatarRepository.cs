using MongoDB.Bson;
using MongoDB.Driver;
using NineChroniclesUtilBackend.Models.Avatar;
using NineChroniclesUtilBackend.Services;

namespace NineChroniclesUtilBackend.Repositories;

public class AvatarRepository(MongoDBCollectionService mongoDBCollectionService)
{
    private readonly IMongoCollection<BsonDocument> _avatarsCollection =
        mongoDBCollectionService.GetCollection<BsonDocument>("avatars");

    public Inventory GetInventory(string avatarAddress)
    {
        var filter = Builders<BsonDocument>.Filter.Eq(f => f["Avatar"]["address"], avatarAddress);
        var projection = Builders<BsonDocument>.Projection.Include(f => f["Avatar"]["inventory"]["Equipments"]);
        var document = _avatarsCollection.Find(filter).Project(projection).FirstOrDefault();
        return new Inventory(document["Avatar"]["inventory"]);
    }
}
