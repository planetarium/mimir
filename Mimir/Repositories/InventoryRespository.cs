using Libplanet.Crypto;
using Mimir.Models.Agent;
using Mimir.Models.Avatar;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class InventoryRepository : BaseRepository<BsonDocument>
{
    public InventoryRepository(MongoDBCollectionService mongoDBCollectionService)
        : base(mongoDBCollectionService)
    {
    }

    protected override string GetCollectionName()
    {
        return "inventory";
    }

    public Inventory? GetInventory(string network, Address avatarAddress)
    {
        var collection = GetCollection(network);
        var filter = Builders<BsonDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            return null;
        }

        try
        {
            return new Inventory(document["State"]["Object"].AsBsonDocument);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
}
