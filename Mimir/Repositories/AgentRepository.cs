using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class AgentRepository(MongoDbService dbService)
{
    public async Task<AgentDocument> GetByAddressAsync(Address agentAddress)
    {
        var collection = dbService.GetCollection<AgentDocument>(CollectionNames.Agent.Value);
        var filter = Builders<AgentDocument>.Filter.Eq("Address", agentAddress.ToHex());
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
