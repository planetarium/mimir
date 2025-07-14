using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.MongoDb;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

using Mimir.MongoDB.Models;

public interface ITransactionRepository
{
    Task<TransactionDocument> GetByTxIdAsync(string txId);
    IExecutable<TransactionDocument> Get();
    IExecutable<TransactionDocument> Get(TransactionFilter? filter);
    IExecutable<TransactionDocument> GetByBlockIndex(long blockIndex);
    IExecutable<TransactionDocument> GetBySignerAsync(string signer);
    IExecutable<TransactionDocument> GetByFirstAvatarAddressInActionArgumentsAsync(string firstAvatarAddress);
    IExecutable<TransactionDocument> GetByFirstActionTypeIdAsync(string firstActionTypeId);
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

    public IExecutable<TransactionDocument> Get(TransactionFilter? filter)
    {
        var filterBuilder = Builders<TransactionDocument>.Filter;
        var filterDefinition = filterBuilder.Empty;

        if (filter?.Signer != null)
        {
            filterDefinition &= filterBuilder.Eq("Object.Signer", filter.Signer);
        }

        if (filter?.FirstAvatarAddressInActionArguments != null)
        {
            filterDefinition &= filterBuilder.Eq("firstAvatarAddressInActionArguments", filter.FirstAvatarAddressInActionArguments);
        }

        if (filter?.FirstActionTypeId != null)
        {
            filterDefinition &= filterBuilder.Eq("firstActionTypeId", filter.FirstActionTypeId);
        }

        if (filter?.BlockIndex != null)
        {
            filterDefinition &= filterBuilder.Eq("BlockIndex", filter.BlockIndex);
        }

        var find = _collection.Find(filterDefinition);
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

    public IExecutable<TransactionDocument> GetBySignerAsync(string signer)
    {
        var filter = Builders<TransactionDocument>.Filter.Eq("Object.Signer", signer);
        var find = _collection.Find(filter);
        var sortDefinition = Builders<TransactionDocument>.Sort.Descending("BlockIndex");
        return find.Sort(sortDefinition).AsExecutable();
    }

    public IExecutable<TransactionDocument> GetByFirstAvatarAddressInActionArgumentsAsync(string firstAvatarAddress)
    {
        var filter = Builders<TransactionDocument>.Filter.Eq("firstAvatarAddressInActionArguments", firstAvatarAddress);
        var find = _collection.Find(filter);
        var sortDefinition = Builders<TransactionDocument>.Sort.Descending("BlockIndex");
        return find.Sort(sortDefinition).AsExecutable();
    }

    public IExecutable<TransactionDocument> GetByFirstActionTypeIdAsync(string firstActionTypeId)
    {
        var filter = Builders<TransactionDocument>.Filter.Eq("firstActionTypeId", firstActionTypeId);
        var find = _collection.Find(filter);
        var sortDefinition = Builders<TransactionDocument>.Sort.Descending("BlockIndex");
        return find.Sort(sortDefinition).AsExecutable();
    }
} 