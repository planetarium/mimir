using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.MongoDb;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Services;
using MongoDB.Bson;
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
    Task<List<DailyActiveUser>> GetDailyActiveUsersAsync(DateTime? startDate = null, DateTime? endDate = null);
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
            var signerFilter = filterBuilder.Eq("Object.Signer", filter.Signer.Value.ToHex());
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

    public async Task<List<DailyActiveUser>> GetDailyActiveUsersAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var pipeline = new List<BsonDocument>();

        var matchStage = new BsonDocument("$match", new BsonDocument
        {
            { "BlockTimestamp", new BsonDocument
                {
                    { "$ne", BsonNull.Value }
                }
            }
        });

        if (startDate.HasValue || endDate.HasValue)
        {
            var timestampFilter = new BsonDocument();
            if (startDate.HasValue)
            {
                timestampFilter.Add("$gte", startDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            }
            if (endDate.HasValue)
            {
                timestampFilter.Add("$lte", endDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            }
            matchStage["$match"]["BlockTimestamp"] = timestampFilter;
        }

        pipeline.Add(matchStage);

        var projectStage = new BsonDocument("$project", new BsonDocument
        {
            { "date", new BsonDocument("$substr", new BsonArray { "$BlockTimestamp", 0, 10 }) },
            { "signer", "$Object.Signer" }
        });
        pipeline.Add(projectStage);

        var groupStage = new BsonDocument("$group", new BsonDocument
        {
            { "_id", new BsonDocument
                {
                    { "date", "$date" },
                    { "signer", "$signer" }
                }
            }
        });
        pipeline.Add(groupStage);

        var groupByDateStage = new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$_id.date" },
            { "count", new BsonDocument("$sum", 1) }
        });
        pipeline.Add(groupByDateStage);

        var sortStage = new BsonDocument("$sort", new BsonDocument("_id", 1));
        pipeline.Add(sortStage);

        var results = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

        return results.Select(doc => new DailyActiveUser(
            doc["_id"].AsString,
            doc["count"].AsInt32
        )).ToList();
    }
} 