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
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{avatarAddress.ToHex()}'");
        }

        try
        {
            return document["State"]["Object"].AsInt32;
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException(
                "document[\"State\"][\"Object\"].AsInt32",
                e);
        }
    }

    protected override string GetCollectionName() => "action_point";
}
