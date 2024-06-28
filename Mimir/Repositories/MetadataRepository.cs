using Lib9c.GraphQL.Enums;
using Mimir.Exceptions;
using Mimir.GraphQL.Extensions;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class MetadataRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    protected override string GetCollectionName() => "metadata";

    public long GetLatestBlockIndex(string network, string pollerType)
    {
        var collection = GetCollection(network);
        return GetLatestBlockIndex(collection, pollerType);
    }

    public long GetLatestBlockIndex(PlanetName planetName, string pollerType)
    {
        var collection = GetCollection(planetName);
        return GetLatestBlockIndex(collection, pollerType);
    }

    private static long GetLatestBlockIndex(
        IMongoCollection<BsonDocument> collection,
        string pollerType)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("PollerType", pollerType);
        var doc = collection.Find(filter).FirstOrDefault();
        if (doc is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'PollerType' equals to '{pollerType}'");
        }

        try
        {
            return doc["LatestBlockIndex"].ToLong();
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException(
                "doc[\"LatestBlockIndex\"]",
                e);
        }
    }
}
