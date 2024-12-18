using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public interface IProductsRepository{
    Task<ProductsStateDocument> GetByAvatarAddressAsync(Address avatarAddress);
}

public class ProductsRepository(IMongoDbService dbService) : IProductsRepository
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
