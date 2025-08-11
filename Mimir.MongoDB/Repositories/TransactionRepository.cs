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
    IExecutable<TransactionDocument> GetByAvatarAddressAsync(string avatarAddress);
    IExecutable<TransactionDocument> GetByActionTypeIdAsync(string actionTypeId);
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
            var signerFilter = filterBuilder.Regex("Object.Signer", filter.Signer.Value.ToHex());
            if (filter.IncludeInvolvedAddress == true)
            {
                var involvedAddressFilter = filterBuilder.AnyEq("extractedActionValues.InvolvedAddresses", filter.Signer.Value.ToHex());
                filterDefinition &= filterBuilder.Or(signerFilter, involvedAddressFilter);
            }
            else
            {
                filterDefinition &= signerFilter;
            }
        }

        if (filter?.AvatarAddress != null)
        {
            var avatarAddressFilter = filterBuilder.Eq("extractedActionValues.AvatarAddress", filter.AvatarAddress.Value.ToHex());
            if (filter.IncludeInvolvedAvatarAddress == true)
            {
                var involvedAvatarAddressFilter = filterBuilder.AnyEq("extractedActionValues.InvolvedAvatarAddresses", filter.AvatarAddress.Value.ToHex());
                filterDefinition &= filterBuilder.Or(avatarAddressFilter, involvedAvatarAddressFilter);
            }
            else
            {
                filterDefinition &= avatarAddressFilter;
            }
        }

        if (filter?.ActionTypeId != null)
        {
            filterDefinition &= filterBuilder.Eq("extractedActionValues.TypeId", filter.ActionTypeId);
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

    public IExecutable<TransactionDocument> GetByAvatarAddressAsync(string avatarAddress)
    {
        var filter = Builders<TransactionDocument>.Filter.Eq("extractedActionValues.AvatarAddress", avatarAddress);
        var find = _collection.Find(filter);
        var sortDefinition = Builders<TransactionDocument>.Sort.Descending("BlockIndex");
        return find.Sort(sortDefinition).AsExecutable();
    }

    public IExecutable<TransactionDocument> GetByActionTypeIdAsync(string actionTypeId)
    {
        var filter = Builders<TransactionDocument>.Filter.Eq("extractedActionValues.TypeId", actionTypeId);
        var find = _collection.Find(filter);
        var sortDefinition = Builders<TransactionDocument>.Sort.Descending("BlockIndex");
        return find.Sort(sortDefinition).AsExecutable();
    }
} 