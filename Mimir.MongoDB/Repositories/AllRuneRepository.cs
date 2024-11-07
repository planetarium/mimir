using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public class AllRuneRepository(MongoDbService dbService)
{
    public async Task<AllRuneDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<AllRuneDocument>();
        var collection = dbService.GetCollection<AllRuneDocument>(collectionName);
        var filter = Builders<AllRuneDocument>.Filter.Eq("_id", address.ToHex());
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
