using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public class StakeRepository(MongoDbService dbService)
{
    public async Task<StakeDocument> GetByAgentAddressAsync(Address agentAddress)
    {
        var collectionName = CollectionNames.GetCollectionName<StakeDocument>();
        var collection = dbService.GetCollection<StakeDocument>(collectionName);
        var filter = Builders<StakeDocument>.Filter.Eq("AgentAddress", agentAddress.ToHex());
        var doc = await collection.Find(filter).FirstOrDefaultAsync();

        if (doc is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'AgentAddress' equals to '{agentAddress.ToHex()}'"
            );
        }

        return doc;
    }
}
