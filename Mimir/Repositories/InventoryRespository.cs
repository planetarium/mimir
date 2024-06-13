using Libplanet.Crypto;
using Mimir.Models;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class InventoryRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
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

    protected override string GetCollectionName() => "inventory";
}
