using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public interface IBlockRepository
{
    Task<BlockDocument> GetByIndexAsync(long index);
}

public class BlockRepository(IMongoDbService dbService) : IBlockRepository
{
    public async Task<BlockDocument> GetByIndexAsync(long index)
    {
        var collection = dbService.GetCollection<BlockDocument>(
            CollectionNames.GetCollectionName<BlockDocument>()
        );
        return await GetBlockInfoAsync(collection, index);
    }

    private static async Task<BlockDocument> GetBlockInfoAsync(
        IMongoCollection<BlockDocument> collection,
        long index
    )
    {
        var builder = Builders<BlockDocument>.Filter;
        var filter = builder.Eq("Object.Index", (int)index);

        var doc = await collection.Find(filter).FirstOrDefaultAsync();
        if (doc is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'index' equals to '{index}'"
            );
        }

        return doc;
    }
}
