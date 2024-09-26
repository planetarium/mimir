using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class ProductsRepository(MongoDbService dbService)
{
    public async Task<ProductsStateDocument> GetByAvatarAddressAsync(Address avatarAddress)
    {
        var collectionName = CollectionNames.GetCollectionName<ProductsStateDocument>();
        var collection = dbService.GetCollection<ProductsStateDocument>(collectionName);
        var filter = Builders<ProductsStateDocument>.Filter.Eq("AvatarAddress", avatarAddress.ToHex());
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
