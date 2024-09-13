using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class ActionPointRepository(MongoDbService dbService)
{
    public async Task<ActionPointDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<ActionPointDocument>();
        var collection = dbService.GetCollection<ActionPointDocument>(collectionName);
        var filter = Builders<ActionPointDocument>.Filter.Eq("Address", address.ToHex());
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
