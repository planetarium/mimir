using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public interface IActionPointRepository
{
    Task<ActionPointDocument> GetByAddressAsync(Address address);
}

public class ActionPointRepository(IMongoDbService dbService) : IActionPointRepository
{
    public virtual async Task<ActionPointDocument> GetByAddressAsync(Address address)
    {
        var collectionName = CollectionNames.GetCollectionName<ActionPointDocument>();
        var collection = dbService.GetCollection<ActionPointDocument>(collectionName);
        var filter = Builders<ActionPointDocument>.Filter.Eq("_id", address.ToHex());
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
