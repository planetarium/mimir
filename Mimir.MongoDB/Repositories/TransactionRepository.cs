using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.MongoDb;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public interface ITransactionRepository
{
    Task<TransactionDocument> GetByTxIdAsync(string txId);
    IExecutable<TransactionDocument> Get();
    IExecutable<TransactionDocument> GetByBlockIndex(long blockIndex);
}

public class TransactionRepository(IMongoDbService dbService) : ITransactionRepository
{
    private readonly IMongoCollection<TransactionDocument> _collection = dbService.GetCollection<TransactionDocument>(
        CollectionNames.GetCollectionName<TransactionDocument>()
    );

    public async Task<TransactionDocument> GetByTxIdAsync(string txId)
    {
        var filter = Builders<TransactionDocument>.Filter.Eq("_id", txId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                _collection.CollectionNamespace.CollectionName,
                $"'TxId' equals to '{txId}'"
            );
        }

        return document;
    }

    public IExecutable<TransactionDocument> Get()
    {
        var find = _collection.Find(Builders<TransactionDocument>.Filter.Empty);
        var sortDefinition = Builders<TransactionDocument>.Sort.Descending("BlockIndex");
        return find.Sort(sortDefinition).AsExecutable();
    }

    public IExecutable<TransactionDocument> GetByBlockIndex(long blockIndex)
    {
        var filter = Builders<TransactionDocument>.Filter.Eq("BlockIndex", blockIndex);
        var find = _collection.Find(filter);
        var sortDefinition = Builders<TransactionDocument>.Sort.Ascending("_id");
        return find.Sort(sortDefinition).AsExecutable();
    }
} 