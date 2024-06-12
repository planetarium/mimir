using System.Text;
using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Constants;
using Mimir.Worker.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nekoyume;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace Mimir.Worker.Services;

public class MongoDbService
{
    private readonly Codec Codec = new();

    private readonly ILogger<MongoDbService> _logger;

    private readonly IMongoClient _client;

    private readonly IMongoDatabase _database;

    private readonly GridFSBucket _gridFs;

    private Dictionary<string, IMongoCollection<BsonDocument>> _stateCollectionMappings =
        new Dictionary<string, IMongoCollection<BsonDocument>>();

    private IMongoCollection<BsonDocument> MetadataCollection =>
        _database.GetCollection<BsonDocument>("metadata");

    public MongoDbService(
        ILogger<MongoDbService> logger,
        string connectionString,
        string databaseName
    )
    {
        _client = new MongoClient(connectionString);
        _database = _client.GetDatabase(databaseName);
        _logger = logger;
        _gridFs = new GridFSBucket(_database);

        _stateCollectionMappings = InitStateCollections();
    }

    private Dictionary<string, IMongoCollection<BsonDocument>> InitStateCollections()
    {
        var mappings = new Dictionary<string, IMongoCollection<BsonDocument>>();

        foreach (var (_, name) in CollectionNames.CollectionMappings)
        {
            mappings[name] = _database.GetCollection<BsonDocument>(name);
        }

        return mappings;
    }

    private IMongoCollection<BsonDocument> GetCollection(string collectionName)
    {
        if (!_stateCollectionMappings.TryGetValue(collectionName, out var collection))
        {
            throw new InvalidOperationException(
                $"No collection mapping found for name: {collectionName}"
            );
        }

        return collection;
    }

    public async Task UpdateLatestBlockIndex(long blockIndex, string pollerType)
    {
        _logger.LogInformation($"Update latest block index to {blockIndex}");
        var filter = Builders<BsonDocument>.Filter.Eq("PollerType", pollerType);
        var update = Builders<BsonDocument>.Update.Set("LatestBlockIndex", blockIndex);

        var response = await MetadataCollection.UpdateOneAsync(filter, update);
        if (response?.ModifiedCount < 1)
        {
            await MetadataCollection.InsertOneAsync(
                new SyncContext
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    PollerType = pollerType,
                    LatestBlockIndex = blockIndex,
                }.ToBsonDocument()
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
        var existingState = await GetCollection(
                CollectionNames.GetCollectionName<WrappedProductsState>()
            )
            .Find(filter)
            .FirstOrDefaultAsync();

        return existingState;
    }

    public async Task<T?> GetSheetAsync<T>()
        where T : ISheet, new()
    {
        var address = Addresses.GetSheetAddress<T>();
        var filter = Builders<BsonDocument>.Filter.Eq("Address", address.ToHex());
        var document = await GetCollection(CollectionNames.GetCollectionName<SheetState>())
            .Find(filter)
            .FirstOrDefaultAsync();

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
        await GetCollection(CollectionNames.GetCollectionName<ProductState>())
            .DeleteOneAsync(productFilter);
    }

    public async Task UpsertStateDataAsyncWithLinkAvatar(
        StateData stateData,
        Address? avatarAddress = null
    )
    {
        var collectionName = CollectionNames.GetCollectionName(stateData.State.GetType());
        var upsertResult = await UpsertStateDataAsync(stateData, collectionName);

        if (upsertResult != null && upsertResult.IsAcknowledged && upsertResult.UpsertedId != null)
        {
            var stateDataObjectId = upsertResult.UpsertedId;

            var avatarCollectionName = CollectionNames.GetCollectionName<AvatarState>();
            var avatarCollection = GetCollection(avatarCollectionName);

            var address = avatarAddress?.ToHex() ?? stateData.Address.ToHex();
            var avatarFilter = Builders<BsonDocument>.Filter.Eq("Address", address);

            var avatarDocument = await avatarCollection.Find(avatarFilter).FirstOrDefaultAsync();
            if (
                avatarDocument != null
                && avatarDocument.Contains($"{collectionName.ToPascalCase()}ObjectId")
            )
            {
                return;
            }

            var update = Builders<BsonDocument>.Update.Set(
                $"{collectionName.ToPascalCase()}ObjectId",
                stateDataObjectId
            );
            await avatarCollection.UpdateOneAsync(avatarFilter, update);
            _logger.LogInformation($"Avatar updated with {collectionName.ToPascalCase()}ObjectId.");
        }
    }

    public async Task<UpdateResult> UpsertStateDataAsync(StateData stateData)
    {
        var collectionName = CollectionNames.GetCollectionName(stateData.State.GetType());
        return await UpsertStateDataAsync(stateData, collectionName);
    }

    private async Task<UpdateResult> UpsertStateDataAsync(
        StateData stateData,
        string collectionName
    )
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Address", stateData.Address.ToHex());
            var bsonDocument = BsonDocument.Parse(stateData.ToJson());
            var update = new BsonDocument("$set", bsonDocument);

            var result = await GetCollection(collectionName)
                .UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

            _logger.LogInformation(
                $"Address: {stateData.Address.ToHex()} - Stored at {collectionName}"
            );
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred during UpsertStateDataAsync: {ex.Message}");
            throw;
        }
    }

    public async Task UpsertTableSheets(StateData stateData, string csv)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Address", stateData.Address.ToHex());

        var sheetCsvBytes = Encoding.UTF8.GetBytes(csv);
        var sheetCsvId = await _gridFs.UploadFromBytesAsync(
            $"{stateData.Address.ToHex()}-csv",
            sheetCsvBytes
        );

        var document = BsonDocument.Parse(stateData.ToJson());
        document.Remove("SheetCsv");
        document.Add("SheetCsvFileId", sheetCsvId);

        var tableSheetCollectionName = CollectionNames.GetCollectionName<SheetState>();
        var tableSheetCollection = GetCollection(tableSheetCollectionName);

        await tableSheetCollection.ReplaceOneAsync(
            filter,
            document,
            new ReplaceOptions { IsUpsert = true }
        );
    }

    private async Task<string> RetrieveFromGridFs(GridFSBucket gridFs, ObjectId fileId)
    {
        var fileBytes = await gridFs.DownloadAsBytesAsync(fileId);
        return Encoding.UTF8.GetString(fileBytes);
    }
}
