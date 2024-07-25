using System.Security.Cryptography.X509Certificates;
using System.Text;
using Bencodex;
using Libplanet.Crypto;
using Lib9c.Models;
using Mimir.Worker.Constants;
using Mimir.Worker.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nekoyume;
using Nekoyume.TableData;
using Serilog;
using AgentState = Mimir.Worker.Models.AgentState;
using AvatarState = Mimir.Worker.Models.AvatarState;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Services;

public class MongoDbService
{
    private readonly MongoClient _client;

    private readonly IMongoDatabase _database;

    private readonly GridFSBucket _gridFs;

    private readonly ILogger _logger;

    private readonly Dictionary<string, IMongoCollection<BsonDocument>> _stateCollectionMappings;

    private IMongoCollection<BsonDocument> MetadataCollection =>
        _database.GetCollection<BsonDocument>("metadata");

    public MongoDbService(string connectionString, string databaseName, string? pathToCAFile)
    {
        var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));

        if (pathToCAFile is not null)
        {
            X509Store localTrustStore = new X509Store(StoreName.Root);
            X509Certificate2Collection certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(pathToCAFile);
            try
            {
                localTrustStore.Open(OpenFlags.ReadWrite);
                localTrustStore.AddRange(certificateCollection);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Root certificate import failed: " + ex.Message);
                throw;
            }
            finally
            {
                localTrustStore.Close();
            }
        }

        _client = new MongoClient(settings);
        _database = _client.GetDatabase(databaseName);
        _gridFs = new GridFSBucket(_database);
        _logger = Log.ForContext<MongoDbService>();
        _stateCollectionMappings = InitStateCollections();
    }

    public MongoClient GetMongoClient()
    {
        return _client;
    }

    private Dictionary<string, IMongoCollection<BsonDocument>> InitStateCollections()
    {
        var mappings = new Dictionary<string, IMongoCollection<BsonDocument>>();
        foreach (var (_, collectionName) in CollectionNames.CollectionAndStateTypeMappings)
        {
            mappings[collectionName] = _database.GetCollection<BsonDocument>(collectionName);
        }

        return mappings;
    }

    public IMongoCollection<BsonDocument> GetCollection(string collectionName)
    {
        if (!_stateCollectionMappings.TryGetValue(collectionName, out var collection))
        {
            throw new InvalidOperationException(
                $"No collection mapping found for name: {collectionName}"
            );
        }

        return collection;
    }

    public IMongoCollection<BsonDocument> GetCollection<T>()
    {
        var collectionName = CollectionNames.GetCollectionName<T>();
        return GetCollection(collectionName);
    }

    public async Task UpdateLatestBlockIndex(
        long blockIndex,
        string pollerType,
        IClientSessionHandle? session = null
    )
    {
        _logger.Debug("Update latest block index to {BlockIndex}", blockIndex);

        var filter = Builders<BsonDocument>.Filter.Eq("PollerType", pollerType);
        var update = Builders<BsonDocument>.Update.Set("LatestBlockIndex", blockIndex);
        if (session is null)
        {
            await MetadataCollection.UpdateOneAsync(
                filter,
                update,
                new UpdateOptions { IsUpsert = true }
            );
        }
        else
        {
            await MetadataCollection.UpdateOneAsync(
                session,
                filter,
                update,
                new UpdateOptions { IsUpsert = true }
            );
        }
    }

    public async Task<long> GetLatestBlockIndex(string pollerType)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("PollerType", pollerType);
        var doc = await MetadataCollection.FindSync(filter).FirstAsync();
        return doc.GetValue("LatestBlockIndex").AsInt64;
    }

    public async Task<BsonDocument> GetProductsStateByAddress(Address address)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Address", address.ToHex());
        return await GetCollection<ProductsState>().Find(filter).FirstOrDefaultAsync();
    }

    public async Task<T?> GetSheetAsync<T>()
        where T : ISheet, new()
    {
        var address = Addresses.GetSheetAddress<T>();
        var filter = Builders<BsonDocument>.Filter.Eq("Address", address.ToHex());
        var document = await GetCollection<SheetState>().Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            return default;
        }

        var csv = await RetrieveFromGridFs(_gridFs, document["SheetCsvFileId"].AsObjectId);
        var sheet = new T();
        sheet.Set(csv);
        return sheet;
    }

    public async Task RemoveProduct(Guid productId)
    {
        var productFilter = Builders<BsonDocument>.Filter.Eq(
            "State.Object.TradableItem.TradableId",
            productId.ToString()
        );
        await GetCollection<ProductState>().DeleteOneAsync(productFilter);
    }

    public async Task<BulkWriteResult> UpsertStateDataManyAsync(
        string collectionName,
        List<StateData> stateDatas,
        IClientSessionHandle? session = null
    )
    {
        List<UpdateOneModel<BsonDocument>> bulkOps = new List<UpdateOneModel<BsonDocument>>();
        foreach (var stateData in stateDatas)
        {
            var stateUpdateModel = GetStateDataUpdateModel(stateData);
            bulkOps.Add(stateUpdateModel);
        }

        if (session is null)
        {
            return await GetCollection(collectionName).BulkWriteAsync(bulkOps);
        }
        else
        {
            return await GetCollection(collectionName).BulkWriteAsync(session, bulkOps);
        }
    }

    public async Task<BulkWriteResult> UpsertStateDataWithRawDataAsync(
        string collectionName,
        List<StateData> stateDatas,
        IClientSessionHandle? session = null
    )
    {
        List<UpdateOneModel<BsonDocument>> bulkOps = new List<UpdateOneModel<BsonDocument>>();
        foreach (var stateData in stateDatas)
        {
            var stateUpdateModel = await GetStateDataWithRawDataUpdateModel(stateData);
            bulkOps.Add(stateUpdateModel);
        }

        if (session is null)
        {
            return await GetCollection(collectionName).BulkWriteAsync(bulkOps);
        }
        else
        {
            return await GetCollection(collectionName).BulkWriteAsync(session, bulkOps);
        }
    }

    public UpdateOneModel<BsonDocument> GetStateDataUpdateModel(StateData stateData)
    {
        var collectionName = CollectionNames.GetCollectionName(stateData.State.GetType());
        var stateJson = stateData.ToJson();
        var bsonDocument = BsonDocument.Parse(stateJson);
        var stateBsonDocument = bsonDocument["State"].AsBsonDocument;
        stateBsonDocument.Remove("Bencoded");

        var filter = Builders<BsonDocument>.Filter.Eq("Address", stateData.Address.ToHex());
        var update = new BsonDocument("$set", bsonDocument);
        var upsertOne = new UpdateOneModel<BsonDocument>(filter, update) { IsUpsert = true };

        _logger.Debug(
            "Address: {Address} - Stored at {CollectionName}",
            stateData.Address.ToHex(),
            collectionName
        );

        return upsertOne;
    }

    public async Task<UpdateOneModel<BsonDocument>> GetStateDataWithRawDataUpdateModel(
        StateData stateData
    )
    {
        var collectionName = CollectionNames.GetCollectionName(stateData.State.GetType());
        var rawStateBytes = new Codec().Encode(stateData.State.Bencoded);
        var rawStateId = await _gridFs.UploadFromBytesAsync(
            $"{stateData.Address.ToHex()}-rawstate",
            rawStateBytes
        );

        var stateJson = stateData.ToJson();
        var bsonDocument = BsonDocument.Parse(stateJson);
        var stateBsonDocument = bsonDocument["State"].AsBsonDocument;
        stateBsonDocument.Remove("Bencoded");
        stateBsonDocument.Add("RawStateFileId", rawStateId);

        var filter = Builders<BsonDocument>.Filter.Eq("Address", stateData.Address.ToHex());
        var update = new BsonDocument("$set", bsonDocument);
        var upsertOne = new UpdateOneModel<BsonDocument>(filter, update) { IsUpsert = true };

        _logger.Debug(
            "Address: {Address} - Stored at {CollectionName}",
            stateData.Address.ToHex(),
            collectionName
        );

        return upsertOne;
    }

    private static async Task<string> RetrieveFromGridFs(GridFSBucket gridFs, ObjectId fileId)
    {
        var fileBytes = await gridFs.DownloadAsBytesAsync(fileId);
        return Encoding.UTF8.GetString(fileBytes);
    }
}
