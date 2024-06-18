using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Models;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class InventoryRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    public Inventory? GetInventory(string network, Address avatarAddress) =>
        GetInventory(GetCollection<BsonDocument>(network), avatarAddress);

    public Inventory? GetInventory(PlanetName planetName, Address avatarAddress) =>
        GetInventory(GetCollection<BsonDocument>(planetName), avatarAddress);

    private static Inventory? GetInventory(
        IMongoCollection<BsonDocument> collection,
        Address avatarAddress)
    {
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
