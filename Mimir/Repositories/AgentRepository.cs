using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.Models;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class AgentRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    public Agent GetAgent(Address agentAddress)
    {
        var collection = GetCollection();
        var filter = Builders<BsonDocument>.Filter.Eq("Address", agentAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{agentAddress.ToHex()}'");
        }

        try
        {
            var doc = document["State"]["Object"].AsBsonDocument;
            return new Agent(doc);
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException(
                "document[\"State\"][\"Object\"].AsBsonDocument",
                e);
        }
    }

    protected override string GetCollectionName() => "agent";
}
