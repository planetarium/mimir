using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class ActionPointRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    public long? GetActionPoint(PlanetName planetName, Address avatarAddress)
    {
        var collection = GetCollection(planetName);
        var filter = Builders<BsonDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            return null;
        }

        try
        {
            return document["State"]["Object"].AsInt32;
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    protected override string GetCollectionName() => "action_point";
}
