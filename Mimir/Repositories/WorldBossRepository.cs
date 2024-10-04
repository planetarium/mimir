using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class WorldBossRepository(MongoDbService dbService)
{
    public async Task<WorldBossStateDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<WorldBossStateDocument>();
        var collection = dbService.GetCollection<WorldBossStateDocument>(collectionName);
        var filter = Builders<WorldBossStateDocument>.Filter.Eq("Address", address.ToHex());
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
