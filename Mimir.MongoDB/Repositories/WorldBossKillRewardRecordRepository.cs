using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public class WorldBossKillRewardRecordRepository(MongoDbService dbService)
{
    public async Task<WorldBossKillRewardRecordDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<WorldBossKillRewardRecordDocument>();
        var collection = dbService.GetCollection<WorldBossKillRewardRecordDocument>(collectionName);
        var filter = Builders<WorldBossKillRewardRecordDocument>.Filter.Eq("Address", address.ToHex());
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
