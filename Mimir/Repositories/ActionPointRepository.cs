using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class ActionPointRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    public int GetActionPoint(PlanetName planetName, Address avatarAddress)
    {
        var collection = GetCollection(planetName);
        var filter = Builders<BsonDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                $"ActionPoint document not found in '{GetCollectionName()}' collection.");
        }

        try
        {
            return document["State"]["Object"].AsInt32;
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException("Invalid key used in ActionPoint document", e);
        }
    }

    protected override string GetCollectionName() => "action_point";
}
