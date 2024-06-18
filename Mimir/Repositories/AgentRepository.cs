using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Models;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class AgentRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    public Agent? GetAgent(PlanetName planetName, Address avatarAddress)
    {
        var collection = GetCollection<BsonDocument>(planetName);
        var filter = Builders<BsonDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            return null;
        }

        try
        {
            var doc = document["State"]["Object"].AsBsonDocument;
            return new Agent(doc);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    protected override string GetCollectionName() => "agent";
}
