using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public interface IBlockRepository
{
    Task<MetadataDocument> GetByIndexAsync(long index);
}

public class BlockRepository(IMongoDbService dbService) : IBlockRepository
{
    public async Task<MetadataDocument> GetByCollectionAsync(string collectionName)
    {
        var collection = dbService.GetCollection<MetadataDocument>(
            CollectionNames.GetCollectionName<MetadataDocument>()
        );
        return await GetLatestBlockIndexAsync(collection, collectionName, null);
    }

    private static async Task<MetadataDocument> GetLatestBlockIndexAsync(
        IMongoCollection<MetadataDocument> collection,
        string collectionName,
        string? pollerType
    )
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
