using System.Security.Cryptography.X509Certificates;
using System.Text;
using Bencodex;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Bson.Serialization;
using Mimir.MongoDB.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nekoyume;
using Nekoyume.TableData;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.MongoDB.Services;

public class MongoDbService : IMongoDbService
{
    private readonly ILogger _logger;
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly GridFSBucket _gridFs;
    private readonly Dictionary<string, IMongoCollection<BsonDocument>> _stateCollectionMappings;
    private readonly IMongoCollection<MetadataDocument> _metadataCollection;
    public readonly IMongoCollection<BlockDocument> _blockCollection;
    public readonly IMongoCollection<TransactionDocument> _transactionCollection;
    private readonly IMongoCollection<ActionTypeDocument> _actionTypeCollection;
    private readonly IMongoCollection<BalanceDocument> _ncgBalanceCollection;

    public MongoDbService(string connectionString, string database, string? pathToCAFile)
    {
        _logger = Log.ForContext<MongoDbService>();
        SerializationRegistry.Register();

        var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));

        if (pathToCAFile is not null)
        {
            var localTrustStore = new X509Store(StoreName.Root);
            var certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(pathToCAFile);
            try
            {
                localTrustStore.Open(OpenFlags.ReadWrite);
                localTrustStore.AddRange(certificateCollection);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Root certificate import failed");
                throw;
            }
            finally
            {
                localTrustStore.Close();
            }
        }

        _client = new MongoClient(settings);
        _database = _client.GetDatabase(database);
        _gridFs = new GridFSBucket(_database);
        _stateCollectionMappings = InitStateCollections();
        _metadataCollection = _database.GetCollection<MetadataDocument>("metadata");
        _blockCollection = _database.GetCollection<BlockDocument>("block");
        _transactionCollection = _database.GetCollection<TransactionDocument>("transaction");
        _actionTypeCollection = _database.GetCollection<ActionTypeDocument>("action_type");
        _ncgBalanceCollection = _database.GetCollection<BalanceDocument>("balance_ncg");
    }

    private Dictionary<string, IMongoCollection<BsonDocument>> InitStateCollections()
    {
        var mappings = new Dictionary<string, IMongoCollection<BsonDocument>>();
        var collectionNames = CollectionNames.CollectionAndStateTypeMappings.Values.Concat(
            CollectionNames.CollectionAndAddressMappings.Values
        );
        var existingCollections = _database.ListCollectionNames().ToList();

        foreach (var collectionName in collectionNames)
        {
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            mappings[collectionName] = collection;

            if (existingCollections.Contains(collectionName))
            {
                continue;
            }

            try
            {
                _database.CreateCollection(collectionName);
            }
            catch (MongoCommandException e)
            {
                // ignore already exists
                _logger.Debug(e, "Collection already exists. {CollectionName}", collectionName);
            }
        }

        return mappings;
    }

    public async Task<UpdateResult> UpdateLatestBlockIndexAsync(
        MetadataDocument document,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    )
    {
        var builder = Builders<MetadataDocument>.Filter;
        var filter = builder.Eq("PollerType", document.PollerType);
        filter &= builder.Eq("CollectionName", document.CollectionName);

        var update = Builders<MetadataDocument>.Update.Set(
            "LatestBlockIndex",
            document.LatestBlockIndex
        );

        var updateOptions = new UpdateOptions { IsUpsert = true };

        var result = session is null
            ? await _metadataCollection.UpdateOneAsync(
                filter,
                update,
                updateOptions,
                cancellationToken
            )
            : await _metadataCollection.UpdateOneAsync(
                session,
                filter,
                update,
                updateOptions,
                cancellationToken
            );

        return result;
    }

    public async Task<long> GetLatestBlockIndexAsync(
        string pollerType,
        string collectionName,
        CancellationToken cancellationToken = default
    )
    {
        var builder = Builders<MetadataDocument>.Filter;
        var filter = builder.Eq("PollerType", pollerType);
        filter &= builder.Eq("CollectionName", collectionName);

        var doc = await _metadataCollection.Find(filter).FirstAsync(cancellationToken);
        return doc.LatestBlockIndex;
    }

    public async Task<bool> IsExistDailyRewardAsync(Address avatarAddress)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", avatarAddress.ToHex());
        var count = await GetCollection<DailyRewardDocument>().CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<bool> IsExistNCGBalanceAsync(Address signer)
    {
        var filter = Builders<BalanceDocument>.Filter.Eq("_id", signer.ToHex());
        var count = await _ncgBalanceCollection.CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<bool> IsExistAgentAsync(Address signer)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", signer.ToHex());
        var count = await GetCollection<AgentDocument>().CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<bool> IsExistAvatarAsync(Address avatarAddress)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", avatarAddress.ToHex());
        var count = await GetCollection<AvatarDocument>().CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<bool> IsExistBlockAsync(long blockIndex)
    {
        var filter = Builders<BlockDocument>.Filter.Eq("Object.Index", blockIndex);
        var count = await _blockCollection.CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<T?> GetSheetAsync<T>(CancellationToken cancellationToken = default)
        where T : ISheet, new()
    {
        var address = Addresses.GetSheetAddress<T>();
        var filter = Builders<BsonDocument>.Filter.Eq("_id", address.ToHex());
        var document = await GetCollection<SheetDocument>()
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
        if (document is null)
        {
            return default;
        }

        var csv = Encoding.UTF8.GetString(await RetrieveFromGridFs(_gridFs, document["RawStateFileId"].AsObjectId));
        var sheet = new T();
        sheet.Set(csv);
        return sheet;
    }

    public Task<BulkWriteResult> UpsertStateDataManyAsync(
        string collectionName,
        List<MimirBsonDocument> documents,
        bool createInsertionDate = false,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    )
    {
        List<WriteModel<BsonDocument>> bulkOps = [];
        foreach (var document in documents)
        {
            var stateUpdateModel = GetMimirDocumentUpdateModel(
                collectionName,
                document,
                createInsertionDate
            );
            bulkOps.Add(stateUpdateModel);
        }

        return UpsertStateDataManyAsync(collectionName, bulkOps, session, cancellationToken);
    }

    public async Task<BulkWriteResult> UpsertBlocksManyAsync(
        List<BlockDocument> documents,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    )
    {
        var bulkOps = documents
            .Select(doc => new ReplaceOneModel<BlockDocument>(
                Builders<BlockDocument>.Filter.Eq(x => x.Id, doc.Id),
                doc
            )
            {
                IsUpsert = true,
            })
            .ToList();

        if (session is null)
        {
            return await _blockCollection.BulkWriteAsync(
                bulkOps,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            return await _blockCollection.BulkWriteAsync(
                session,
                bulkOps,
                cancellationToken: cancellationToken
            );
        }
    }

    public async Task<BulkWriteResult> UpsertTransactionsManyAsync(
        List<TransactionDocument> documents,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    )
    {
        var bulkOps = documents
            .Select(doc => new ReplaceOneModel<TransactionDocument>(
                Builders<TransactionDocument>.Filter.Eq(x => x.Id, doc.Id),
                doc
            )
            {
                IsUpsert = true,
            })
            .ToList();

        if (session is null)
        {
            return await _transactionCollection.BulkWriteAsync(
                bulkOps,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            return await _transactionCollection.BulkWriteAsync(
                session,
                bulkOps,
                cancellationToken: cancellationToken
            );
        }
    }

    public async Task<BulkWriteResult> UpsertStateDataManyAsync(
        string collectionName,
        List<WriteModel<BsonDocument>> bulkOps,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    )
    {
        return session is null
            ? await GetCollection(collectionName).BulkWriteAsync(bulkOps, null, cancellationToken)
            : await GetCollection(collectionName)
                .BulkWriteAsync(session, bulkOps, null, cancellationToken);
    }

    public async Task<BulkWriteResult> UpsertSheetDocumentAsync(
        string collectionName,
        List<SheetDocument> documents,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    )
    {
        List<UpdateOneModel<BsonDocument>> bulkOps = [];
        foreach (var document in documents)
        {
            var stateUpdateModel = await GetSheetDocumentUpdateModelAsync(
                document,
                cancellationToken
            );
            bulkOps.Add(stateUpdateModel);
        }

        return session is null
            ? await GetCollection(collectionName).BulkWriteAsync(bulkOps, null, cancellationToken)
            : await GetCollection(collectionName)
                .BulkWriteAsync(session, bulkOps, null, cancellationToken);
    }

    public async Task UpsertActionTypeAsync(string actionTypeId)
    {
        var filter = Builders<ActionTypeDocument>.Filter.Eq("_id", actionTypeId);
        var update = Builders<ActionTypeDocument>.Update.SetOnInsert("_id", actionTypeId);

        var options = new UpdateOptions { IsUpsert = true };
        await _actionTypeCollection.UpdateOneAsync(filter, update, options);
    }

    private UpdateOneModel<BsonDocument> GetMimirDocumentUpdateModel(
        string collectionName,
        MimirBsonDocument document,
        bool createInsertionDate = false
    )
    {
        var json = document.ToJson();
        var bsonDocument = BsonDocument.Parse(json);
        if (createInsertionDate)
        {
            bsonDocument["InsertionDate"] = DateTime.UtcNow;
        }

        var filter = Builders<BsonDocument>.Filter.Eq("_id", document.Id);
        var update = new BsonDocument("$set", bsonDocument);
        var upsertOne = new UpdateOneModel<BsonDocument>(filter, update) { IsUpsert = true };

        _logger.Debug(
            "Address: {Address} - Stored at {CollectionName}",
            document.Id,
            collectionName
        );

        return upsertOne;
    }

    private async Task<UpdateOneModel<BsonDocument>> GetSheetDocumentUpdateModelAsync(
        SheetDocument document,
        CancellationToken cancellationToken = default
    )
    {
        var collectionName = CollectionNames.GetCollectionName(document.GetType());
        var rawStateBytes = new Codec().Encode(document.RawState);
        var rawStateId = await _gridFs.UploadFromBytesAsync(
            $"{document.Address.ToHex()}-rawstate",
            rawStateBytes,
            null,
            cancellationToken
        );

        var json = MimirBsonDocumentExtensions.ToJson(document);
        var bsonDocument = BsonDocument.Parse(json);
        bsonDocument.Remove("RawState");
        bsonDocument.Add("RawStateFileId", rawStateId);

        var filter = Builders<BsonDocument>.Filter.Eq("_id", document.Address.ToHex());
        var update = new BsonDocument("$set", bsonDocument);
        var upsertOne = new UpdateOneModel<BsonDocument>(filter, update) { IsUpsert = true };

        _logger.Debug(
            "Address: {Address} - Stored at {CollectionName}",
            document.Address.ToHex(),
            collectionName
        );

        return upsertOne;
    }

    public static async Task<byte[]> RetrieveFromGridFs(GridFSBucket gridFs, ObjectId fileId)
    {
        var fileBytes = await gridFs.DownloadAsBytesAsync(fileId);
        return fileBytes;
    }

    public IMongoDatabase GetDatabase()
    {
        return _database;
    }

    public GridFSBucket GetGridFs()
    {
        return _gridFs;
    }

    public async Task<byte[]> RetrieveFromGridFs(ObjectId fileId)
    {
        return await RetrieveFromGridFs(_gridFs, fileId);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        var database = GetDatabase();
        return database.GetCollection<T>(collectionName);
    }

    public IMongoCollection<BsonDocument> GetCollection(string collectionName)
    {
        if (_stateCollectionMappings.TryGetValue(collectionName, out var collection))
        {
            return collection;
        }

        throw new InvalidOperationException(
            $"No collection mapping found for name: {collectionName}"
        );
    }

    public IMongoCollection<BsonDocument> GetCollection<T>()
    {
        var collectionName = CollectionNames.GetCollectionName<T>();
        return GetCollection(collectionName);
    }

}
