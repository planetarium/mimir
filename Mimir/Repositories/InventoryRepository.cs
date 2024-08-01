using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.Models;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class InventoryRepository(MongoDbService dbService)
{
    public Inventory GetInventory(Address avatarAddress) =>
        GetInventory(
            dbService.GetCollection<BsonDocument>(CollectionNames.Inventory.Value),
            avatarAddress
        );

    private static Inventory GetInventory(
        IMongoCollection<BsonDocument> collection,
        Address avatarAddress
    )
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{avatarAddress.ToHex()}'"
            );
        }

        try
        {
            var doc = document["Object"].AsBsonDocument;
            return new Inventory(doc);
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException(
                "document[\"State\"][\"Object\"].AsBsonDocument",
                e
            );
        }
    }
}
