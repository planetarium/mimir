using Mimir.Models.Avatar;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class AvatarRepository : BaseRepository<BsonDocument>
{
    public AvatarRepository(MongoDBCollectionService mongoDBCollectionService)
        : base(mongoDBCollectionService)
    {
    }

    protected override string GetCollectionName()
    {
        return "avatars";
    }

    public Inventory? GetInventory(string network, string avatarAddress)
    {
        var collection = GetCollection(network);
        var filter = Builders<BsonDocument>.Filter.Eq("Avatar.address", avatarAddress);
        var projection = Builders<BsonDocument>.Projection.Include("Avatar.inventory.Equipments");
        var document = collection.Find(filter).Project(projection).FirstOrDefault();
        if (document is null)
        {
            return null;
        }

        try
        {
            return new Inventory(document["Avatar"]["inventory"].AsBsonDocument);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
}
