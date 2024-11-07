using Libplanet.Crypto;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public class AgentRepository(MongoDbService dbService)
{
    public async Task<AgentDocument> GetByAddressAsync(Address agentAddress)
    {
        var collectionName = CollectionNames.GetCollectionName<AgentDocument>();
        var collection = dbService.GetCollection<AgentDocument>(collectionName);
        var filter = Builders<AgentDocument>.Filter.Eq("_id", agentAddress.ToHex());
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{agentAddress.ToHex()}'");
        }

        return document;
    }
}
