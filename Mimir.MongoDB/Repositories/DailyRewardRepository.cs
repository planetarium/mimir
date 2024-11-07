using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public class DailyRewardRepository(MongoDbService dbService)
{
    public async Task<DailyRewardDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<DailyRewardDocument>();
        var collection = dbService.GetCollection<DailyRewardDocument>(collectionName);
        var filter = Builders<DailyRewardDocument>.Filter.Eq("_id", address.ToHex());
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
