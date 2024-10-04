using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class WorldBossRaiderRepository(MongoDbService dbService)
{
    public async Task<RaiderStateDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<RaiderStateDocument>();
        var collection = dbService.GetCollection<RaiderStateDocument>(collectionName);
        var filter = Builders<RaiderStateDocument>.Filter.Eq("Address", address.ToHex());
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
