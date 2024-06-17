using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.Models;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class InventoryRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    public Inventory GetInventory(string network, Address avatarAddress) =>
        GetInventory(GetCollection(network), avatarAddress);

    public Inventory GetInventory(PlanetName planetName, Address avatarAddress) =>
        GetInventory(GetCollection(planetName), avatarAddress);

    private static Inventory GetInventory(
        IMongoCollection<BsonDocument> collection,
        Address avatarAddress)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{avatarAddress.ToHex()}'");
        }

        try
        {
            return new Inventory(document["State"]["Object"].AsBsonDocument);
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException("Invalid key used in Inventory document", e);
        }
    }

    protected override string GetCollectionName() => "inventory";
}
