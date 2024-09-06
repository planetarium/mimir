using System.Security.Cryptography.X509Certificates;
using System.Text;
using Bencodex;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Bson.Serialization;
using Mimir.Worker.Constants;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nekoyume;
using Nekoyume.TableData;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Services;

public class MongoDbService
{
    private readonly MongoClient _client;

    private readonly IMongoDatabase _database;

    private readonly GridFSBucket _gridFs;

    private readonly ILogger _logger;

    private readonly Dictionary<string, IMongoCollection<BsonDocument>> _stateCollectionMappings;

    private IMongoCollection<MetadataDocument> MetadataCollection =>
        _database.GetCollection<MetadataDocument>("metadata");

    public MongoDbService(string connectionString, PlanetType planetType, string? pathToCAFile)
    {
        SerializationRegistry.Register();

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
        _database = _client.GetDatabase(planetType.ToString());
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
        foreach (
            var collectionName in CollectionNames.CollectionAndStateTypeMappings.Values.Concat(
                CollectionNames.CollectionAndAddressMappings.Values
            )
        )
        {
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            mappings[collectionName] = collection;

            var count = collection.CountDocuments(new BsonDocument());

            if (count == 0)
            {
                try
                {
                    _database.CreateCollection(collectionName);
                }
                catch (MongoCommandException)
                {
                    // ignore already exists
                }
            }
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

    public async Task<long> GetArenaDocumentCount(int championshipId, int round)
    {
        var builder = Builders<BsonDocument>.Filter;
        var filter = builder.Eq("ChampionshipId", championshipId);
        filter &= builder.Eq("Round", round);

        var documents = GetCollection<ArenaDocument>().Find(filter);
        var count = await documents.CountDocumentsAsync();

        return count;
    }

    public IMongoCollection<BsonDocument> GetCollection<T>()
    {
        var collectionName = CollectionNames.GetCollectionName<T>();
        return GetCollection(collectionName);
    }

    public async Task<UpdateResult> UpdateLatestBlockIndex(
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
            ? await MetadataCollection.UpdateOneAsync(
                filter,
                update,
                updateOptions,
                cancellationToken
            )
            : await MetadataCollection.UpdateOneAsync(
                session,
                filter,
                update,
                updateOptions,
                cancellationToken
            );

        return result;
    }

    public async Task<long> GetLatestBlockIndex(
        string pollerType,
        string collectionName,
        CancellationToken cancellationToken = default
    )
    {
        var builder = Builders<MetadataDocument>.Filter;
        var filter = builder.Eq("PollerType", pollerType);
        filter &= builder.Eq("CollectionName", collectionName);

        var doc = await MetadataCollection.FindSync(filter).FirstAsync(cancellationToken);
        return doc.LatestBlockIndex;
    }

    public async Task<T?> GetSheetAsync<T>(CancellationToken cancellationToken = default)
        where T : ISheet, new()
    {
        var address = Addresses.GetSheetAddress<T>();
        var filter = Builders<BsonDocument>.Filter.Eq("Address", address.ToHex());
        var document = await GetCollection<SheetDocument>()
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
        if (document is null)
        {
            return default;
        }

        var csv = await RetrieveFromGridFs(_gridFs, document["RawStateFileId"].AsObjectId);
        var sheet = new T();
        sheet.Set(csv);
        return sheet;
    }

    public async Task<BulkWriteResult> UpsertStateDataManyAsync(
        string collectionName,
        List<MimirBsonDocument> documents,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    )
    {
        List<UpdateOneModel<BsonDocument>> bulkOps = [];
        foreach (var document in documents)
        {
            var stateUpdateModel = GetMimirDocumentUpdateModel(document);
            bulkOps.Add(stateUpdateModel);
        }

        if (session is null)
        {
            return await GetCollection(collectionName)
                .BulkWriteAsync(bulkOps, null, cancellationToken);
        }
        else
        {
            return await GetCollection(collectionName)
                .BulkWriteAsync(session, bulkOps, null, cancellationToken);
        }
    }

    public async Task<BulkWriteResult> UpsertSheetDocumentAsync(
        string collectionName,
        List<SheetDocument> documents,
        IClientSessionHandle? session = null,
        CancellationToken cancellationToken = default
    )
    {
        List<UpdateOneModel<BsonDocument>> bulkOps = new List<UpdateOneModel<BsonDocument>>();
        foreach (var document in documents)
        {
            var stateUpdateModel = await GetSheetDocumentUpdateModel(document);
            bulkOps.Add(stateUpdateModel);
        }

        if (session is null)
        {
            return await GetCollection(collectionName)
                .BulkWriteAsync(bulkOps, null, cancellationToken);
        }
        else
        {
            return await GetCollection(collectionName)
                .BulkWriteAsync(session, bulkOps, null, cancellationToken);
        }
    }

    public UpdateOneModel<BsonDocument> GetMimirDocumentUpdateModel(MimirBsonDocument document)
    {
        var collectionName = CollectionNames.GetCollectionName(document.GetType());
        var filter = Builders<BsonDocument>.Filter
            .Eq("Address", document.Address.ToHex());
        var stateJson = document.ToJson();
        var bsonDocument = BsonDocument.Parse(stateJson);
        var update = new BsonDocument("$set", bsonDocument);
        // var update = Builders<BsonDocument>.Update
        //     .Set(doc => doc, document);
        var upsertOne = new UpdateOneModel<BsonDocument>(filter, update) { IsUpsert = true };

        _logger.Debug(
            "Address: {Address} - Stored at {CollectionName}",
            document.Address.ToHex(),
            collectionName
        );

        return upsertOne;
    }

    public async Task<UpdateOneModel<BsonDocument>> GetSheetDocumentUpdateModel(
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

        var bsonDocument = BsonDocument.Parse(document.ToJson());
        var stateBsonDocument = bsonDocument.AsBsonDocument;
        stateBsonDocument.Remove("RawState");
        stateBsonDocument.Add("RawStateFileId", rawStateId);

        var filter = Builders<BsonDocument>.Filter.Eq("Address", document.Address.ToHex());
        var update = new BsonDocument("$set", stateBsonDocument);
        var upsertOne = new UpdateOneModel<BsonDocument>(filter, update) { IsUpsert = true };

        _logger.Debug(
            "Address: {Address} - Stored at {CollectionName}",
            document.Address.ToHex(),
            collectionName
        );

        return upsertOne;
    }

    public async Task<BsonDocument> GetProductsStateByAddress(Address address)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Address", address.ToHex());
        return await GetCollection<ProductsStateDocument>().Find(filter).FirstOrDefaultAsync();
    }

    public async Task RemoveProduct(Guid productId)
    {
        var productFilter = Builders<BsonDocument>.Filter.Eq(
            "Object.TradableItem.TradableId",
            productId.ToString()
        );
        await GetCollection<ProductDocument>().DeleteOneAsync(productFilter);
    }

    private static async Task<string> RetrieveFromGridFs(GridFSBucket gridFs, ObjectId fileId)
    {
        var fileBytes = await gridFs.DownloadAsBytesAsync(fileId);
        return Encoding.UTF8.GetString(fileBytes);
    }
}
