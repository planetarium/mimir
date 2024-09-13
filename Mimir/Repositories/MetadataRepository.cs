using Mimir.Exceptions;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class MetadataRepository(MongoDbService dbService)
{
    public async Task<MetadataDocument> GetByCollectionAsync(string collectionName)
    {
        var collection = dbService.GetCollection<MetadataDocument>(
            CollectionNames.GetCollectionName<MetadataDocument>());
        return await GetLatestBlockIndexAsync(collection, collectionName, null);
    }

    public async Task<MetadataDocument> GetByCollectionAndTypeAsync(
        string pollerType,
        string collectionName)
    {
        var collection = dbService.GetCollection<MetadataDocument>(
            CollectionNames.GetCollectionName<MetadataDocument>());
        return await GetLatestBlockIndexAsync(collection, collectionName, pollerType);
    }

    private static async Task<MetadataDocument> GetLatestBlockIndexAsync(
        IMongoCollection<MetadataDocument> collection,
        string collectionName,
        string? pollerType)
    {
        var builder = Builders<MetadataDocument>.Filter;
        var filter = builder.Eq("CollectionName", collectionName);
        if (pollerType is not null)
        {
            filter &= builder.Eq("PollerType", pollerType);
        }

        var doc = await collection.Find(filter).FirstOrDefaultAsync();
        if (doc is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'PollerType' equals to '{pollerType}'"
            );
        }

        return doc;
    }
}
