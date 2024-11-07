using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public class InventoryRepository(MongoDbService dbService)
{
    public async Task<InventoryDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<InventoryDocument>();
        var collection = dbService.GetCollection<InventoryDocument>(collectionName);
        var filter = Builders<InventoryDocument>.Filter.Eq("_id", address.ToHex());
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{address.ToHex()}'");
        }

        return document;
    }
}
