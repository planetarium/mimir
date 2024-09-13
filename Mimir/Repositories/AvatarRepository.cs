using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class AvatarRepository(MongoDbService dbService)
{
    public async Task<AvatarDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<AvatarDocument>();
        var collection = dbService.GetCollection<AvatarDocument>(collectionName);
        var filter = Builders<AvatarDocument>.Filter.Eq("Address", address.ToHex());
        var doc = await collection.Find(filter).FirstOrDefaultAsync();
        if (doc is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{address.ToHex()}'"
            );
        }

        return doc;
    }
}
