using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class MetadataRepository(MongoDbService dbService)
{
    public long GetLatestBlockIndex(string pollerType, string collectionName)
    {
        var collection = dbService.GetCollection<MetadataDocument>(CollectionNames.Metadata.Value);
        return GetLatestBlockIndex(collection, pollerType, collectionName);
    }

    private static long GetLatestBlockIndex(
        IMongoCollection<MetadataDocument> collection,
        string pollerType,
        string collectionName
    )
    {
        var builder = Builders<MetadataDocument>.Filter;
        var filter = builder.Eq("PollerType", pollerType);
        filter &= builder.Eq("CollectionName", collectionName);

        var doc = collection.Find(filter).FirstOrDefault();
        if (doc is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'PollerType' equals to '{pollerType}'"
            );
        }

        return doc.LatestBlockIndex;
    }
}
