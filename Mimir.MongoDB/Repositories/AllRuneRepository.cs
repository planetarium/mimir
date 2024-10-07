using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public class AllRuneRepository(MongoDbService dbService)
{
    public Task<AllRuneDocument> GetByAddressAsync(Address avatarAddress)
    {
        var collectionName = CollectionNames.GetCollectionName<AllRuneDocument>();
        var collection = dbService.GetCollection<AllRuneDocument>(collectionName);
        var filter = Builders<AllRuneDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{avatarAddress.ToHex()}'");
        }

        return document;
    }
}
