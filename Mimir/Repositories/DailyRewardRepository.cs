using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.MongoDB.Bson.Extensions;
using Mimir.MongoDB.Exceptions;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class DailyRewardRepository(MongoDbService dbService)
{
    public async Task<long> GetByAddressAsync(Address address)
    {
        var collection = dbService.GetCollection<BsonDocument>(CollectionNames.DailyReward.Value);
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
            return document["Object"].ToLong();
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException(
                "document[\"State\"][\"Object\"]",
                e);
        }
    }
}
