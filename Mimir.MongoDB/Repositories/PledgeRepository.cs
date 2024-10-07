using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public class PledgeRepository(MongoDbService dbService)
{
    public async Task<PledgeDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<PledgeDocument>();
        var collection = dbService.GetCollection<PledgeDocument>(collectionName);
        var filter = Builders<PledgeDocument>.Filter.Eq("Address", address.ToHex());
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
