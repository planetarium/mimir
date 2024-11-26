using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public class CollectionRepository(IMongoDbService dbService)
{
    public async Task<CollectionDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<CollectionDocument>();
        var collection = dbService.GetCollection<CollectionDocument>(collectionName);
        var filter = Builders<CollectionDocument>.Filter.Eq("_id", address.ToHex());
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
