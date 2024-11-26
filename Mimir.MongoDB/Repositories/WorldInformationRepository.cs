using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public class WorldInformationRepository(IMongoDbService dbService)
{
    public async Task<WorldInformationDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<WorldInformationDocument>();
        var collection = dbService.GetCollection<WorldInformationDocument>(collectionName);
        var filter = Builders<WorldInformationDocument>.Filter.Eq(
            "_id",
            address.ToHex()
        );
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{address.ToHex()}'"
            );
        }

        return document;
    }
}
