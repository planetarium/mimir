using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Bencodex;
using Libplanet.Crypto;
using Mimir.Models.Abstractions;
using Mimir.Worker.Constants;
using Mimir.Worker.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nekoyume;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using Serilog;
using AgentState = Mimir.Worker.Models.AgentState;
using AvatarState = Mimir.Worker.Models.AvatarState;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Services;

public class MongoDbService
{
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

        _database = new MongoClient(settings).GetDatabase(databaseName);
        _gridFs = new GridFSBucket(_database);
        _logger = Log.ForContext<MongoDbService>();
        _stateCollectionMappings = InitStateCollections();
    }

    private Dictionary<string, IMongoCollection<BsonDocument>> InitStateCollections()
    {
        var mappings = new Dictionary<string, IMongoCollection<BsonDocument>>();
        foreach (var (_, collectionName) in CollectionNames.CollectionMappings)
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

    public async Task UpdateLatestBlockIndex(long blockIndex, string pollerType)
    {
        _logger.Debug("Update latest block index to {BlockIndex}", blockIndex);

        var filter = Builders<BsonDocument>.Filter.Eq("PollerType", pollerType);
        var update = Builders<BsonDocument>.Update.Set("LatestBlockIndex", blockIndex);
        await MetadataCollection.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = true }
        );
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

    public async Task UpsertStateDataAsyncWithLinkAgent(
        StateData stateData,
        Address? agentAddress = null
    )
    {
        var collectionName = CollectionNames.GetCollectionName(stateData.State.GetType());
        var upsertResult = await UpsertStateDataAsync(stateData, collectionName);
        await LinkToOtherCollectionAsync<AgentState>(
            upsertResult,
            agentAddress ?? stateData.Address,
            collectionName
        );
    }

    public async Task UpsertStateDataAsyncWithLinkAvatar(
        StateData stateData,
        Address? avatarAddress = null
    )
    {
        var collectionName = CollectionNames.GetCollectionName(stateData.State.GetType());
        var upsertResult = await UpsertStateDataAsync(stateData, collectionName);
        await LinkToOtherCollectionAsync<AvatarState>(
            upsertResult,
            avatarAddress ?? stateData.Address,
            collectionName
        );
    }

    public async Task UpsertStateDataAsync(StateData stateData)
    {
        var collectionName = CollectionNames.GetCollectionName(stateData.State.GetType());
        await UpsertStateDataAsync(stateData, collectionName);
    }

    private async Task<UpdateResult> UpsertStateDataAsync(
        StateData stateData,
        string collectionName
    )
    {
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
        var result = await GetCollection(collectionName)
            .UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

        _logger.Debug(
            "Address: {Address} - Stored at {CollectionName}",
            stateData.Address.ToHex(),
            collectionName
        );
        return result;
    }

    private static async Task<string> RetrieveFromGridFs(GridFSBucket gridFs, ObjectId fileId)
    {
        var fileBytes = await gridFs.DownloadAsBytesAsync(fileId);
        return Encoding.UTF8.GetString(fileBytes);
    }

    public async Task UpsertStateModelAsync<T>(T stateModel)
        where T : IStateModel
    {
        var collectionName = CollectionNames.GetCollectionName<T>();
        await UpsertStateModelAsync(stateModel, collectionName);
    }

    private async Task<UpdateResult> UpsertStateModelAsync<T>(T stateModel, string collectionName)
        where T : IStateModel
    {
        var addr = stateModel.Address.ToHex();
        var filter = Builders<BsonDocument>.Filter.Eq("Address", addr);
        var bsonDocument = stateModel.ToBsonDocument();
        var update = new BsonDocument("$set", bsonDocument);
        var result = await GetCollection(collectionName)
            .UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

        _logger.Debug("Address: {Address} - Stored at {CollectionName}", addr, collectionName);
        return result;
    }

    private async Task LinkToOtherCollectionAsync<T>(
        UpdateResult upsertResult,
        Address address,
        string collectionName
    )
    {
        if (!upsertResult.IsAcknowledged || upsertResult.UpsertedId == null)
        {
            return;
        }

        var collection = GetCollection<T>();
        var filter = Builders<BsonDocument>.Filter.Eq("Address", address.ToHex());
        var doc = await collection.Find(filter).FirstOrDefaultAsync();
        if (doc is null)
        {
            return;
        }

        var field = $"{collectionName.ToPascalCase()}ObjectId";
        if (doc.Contains(field))
        {
            return;
        }

        var update = Builders<BsonDocument>.Update.Set(field, upsertResult.UpsertedId);
        await collection.UpdateOneAsync(filter, update);
        _logger.Debug("{TypeName} updated with {CollectionNameObjectId}", typeof(T).Name, field);
    }
}
