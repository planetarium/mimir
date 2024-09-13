using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class InventoryRepository(MongoDbService dbService)
{
    public async Task<InventoryDocument> GetByAddressAsync(Address avatarAddress)
    {
        var collectionName = CollectionNames.GetCollectionName<InventoryDocument>();
        var collection = dbService.GetCollection<InventoryDocument>(collectionName);
        var filter = Builders<InventoryDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{avatarAddress.ToHex()}'");
        }

        return document;
    }
}
