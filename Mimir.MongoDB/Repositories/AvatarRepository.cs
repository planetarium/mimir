using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public class AvatarRepository(IMongoDbService dbService) : IAvatarRepository
{
    public virtual async Task<AvatarDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<AvatarDocument>();
        var collection = dbService.GetCollection<AvatarDocument>(collectionName);
        var filter = Builders<AvatarDocument>.Filter.Eq("_id", address.ToHex());
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
