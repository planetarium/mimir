using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Libplanet.Crypto;
using Nekoyume.TableData;
using Mimir.MongoDB.Bson;

namespace Mimir.MongoDB.Services;

public interface IMongoDbService
{
    Task<UpdateResult> UpdateLatestBlockIndexAsync(
        MetadataDocument document,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    );

    Task<long> GetLatestBlockIndexAsync(
        string pollerType,
        string collectionName,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsExistDailyRewardAsync(Address avatarAddress);

    Task<bool> IsExistNCGBalanceAsync(Address signer);

    Task<bool> IsExistAgentAsync(Address signer);

    Task<bool> IsExistAvatarAsync(Address avatarAddress);

    Task<bool> IsExistBlockAsync(long blockIndex);

    Task<T?> GetSheetAsync<T>(CancellationToken cancellationToken = default)
        where T : ISheet, new();

    Task<BulkWriteResult> UpsertStateDataManyAsync(
        string collectionName,
        List<MimirBsonDocument> documents,
        bool createInsertionDate = false,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    );

    Task<BulkWriteResult> UpsertBlocksManyAsync(
        List<BlockDocument> documents,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    );

    Task<BulkWriteResult> UpsertTransactionsManyAsync(
        List<TransactionDocument> documents,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    );

    Task<BulkWriteResult> UpsertStateDataManyAsync(
        string collectionName,
        List<WriteModel<BsonDocument>> bulkOps,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    );

    Task<BulkWriteResult> UpsertSheetDocumentAsync(
        string collectionName,
        List<SheetDocument> documents,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    );

    Task UpsertActionTypeAsync(string actionTypeId);

    IMongoCollection<T> GetCollection<T>(string collectionName);
    
    IMongoCollection<BsonDocument> GetCollection(string collectionName);
    
    IMongoCollection<BsonDocument> GetCollection<T>();

    IMongoDatabase GetDatabase();
    
    GridFSBucket GetGridFs();
    
    Task<byte[]> RetrieveFromGridFs(ObjectId fileId);
}
