using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.MongoDB.Exceptions;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class ActionPointRepository(MongoDbService dbService)
{
    public async Task<int> GetByAddressAsync(Address address)
    {
        var collection = dbService.GetCollection<BsonDocument>(CollectionNames.ActionPoint.Value);
        var filter = Builders<BsonDocument>.Filter.Eq("Address", address.ToHex());
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{address.ToHex()}'");
        }

        try
        {
            return document["Object"].AsInt32;
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException(
                "document[\"State\"][\"Object\"].AsInt32",
                e);
        }
    }
}
