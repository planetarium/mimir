using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.MongoDb;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Models;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public interface IBlockRepository
{
    Task<BlockDocument> GetByIndexAsync(long index);
    IExecutable<BlockDocument> Get();
    IExecutable<BlockDocument> Get(BlockFilter? filter);
}

public class BlockRepository(IMongoDbService dbService) : IBlockRepository
{
    private readonly IMongoCollection<BlockDocument> _collection = dbService.GetCollection<BlockDocument>(
        CollectionNames.GetCollectionName<BlockDocument>()
    );

    public async Task<BlockDocument> GetByIndexAsync(long index)
    {
        return await GetBlockInfoAsync(_collection, index);
    }

    public IExecutable<BlockDocument> Get()
    {
        var find = _collection.Find(Builders<BlockDocument>.Filter.Empty);
        var sortDefinition = Builders<BlockDocument>.Sort.Descending("Object.Index");
        return find.Sort(sortDefinition).AsExecutable();
    }

    public IExecutable<BlockDocument> Get(BlockFilter? filter)
    {
        var filterBuilder = Builders<BlockDocument>.Filter;
        var filterDefinition = filterBuilder.Empty;

        if (filter?.Miner != null)
        {
            filterDefinition &= filterBuilder.Eq("Object.Miner", filter.Miner.Value.ToHex());
        }

        var find = _collection.Find(filterDefinition);
        var sortDefinition = Builders<BlockDocument>.Sort.Descending("Object.Index");
        return find.Sort(sortDefinition).AsExecutable();
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
