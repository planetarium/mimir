using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public interface IAllCombinationSlotStateRepository
{
    Task<AllCombinationSlotStateDocument> GetByAddressAsync(Address address);
}

public class AllCombinationSlotStateRepository(IMongoDbService dbService) : IAllCombinationSlotStateRepository
{
    public async Task<AllCombinationSlotStateDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<AllCombinationSlotStateDocument>();
        var collection = dbService.GetCollection<AllCombinationSlotStateDocument>(collectionName);
        var filter = Builders<AllCombinationSlotStateDocument>.Filter.Eq("_id", address.ToHex());
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
