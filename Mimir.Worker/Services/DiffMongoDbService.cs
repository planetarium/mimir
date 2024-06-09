using System.Text;
using Libplanet.Crypto;
using Mimir.Worker.Constants;
using Mimir.Worker.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nekoyume.Model.State;

namespace Mimir.Worker.Services;

public class DiffMongoDbService
{
    private readonly ILogger<DiffMongoDbService> _logger;

    private readonly IMongoClient _client;

    private readonly IMongoDatabase _database;

    private readonly GridFSBucket _gridFs;

    private Dictionary<string, IMongoCollection<BsonDocument>> _stateCollectionMappings =
        new Dictionary<string, IMongoCollection<BsonDocument>>();

    private IMongoCollection<BsonDocument> MetadataCollection =>
        _database.GetCollection<BsonDocument>("metadata");

    public DiffMongoDbService(
        ILogger<DiffMongoDbService> logger,
        string connectionString,
        string databaseName
    )
    {
        _client = new MongoClient(connectionString);
        _database = _client.GetDatabase(databaseName);
        _logger = logger;
        _gridFs = new GridFSBucket(_database);

        InitStateCollections();
    }

    private void InitStateCollections()
    {
        foreach (var (_, name) in CollectionNames.CollectionMappings)
        {
            IMongoCollection<BsonDocument> Collection = _database.GetCollection<BsonDocument>(name);
            _stateCollectionMappings.Add(name, Collection);
        }
    }

    public IMongoCollection<BsonDocument> GetStateCollection(string collectionName)
    {
        return _stateCollectionMappings[collectionName];
    }

    public async Task UpdateLatestBlockIndex(long blockIndex, string? id = "SyncContext")
    {
        _logger.LogInformation($"Update latest block index to {blockIndex}");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
        var update = Builders<BsonDocument>.Update.Set("LatestBlockIndex", blockIndex);

        var response = await MetadataCollection.UpdateOneAsync(filter, update);
        if (response?.ModifiedCount < 1)
        {
            await MetadataCollection.InsertOneAsync(
                new SyncContext { LatestBlockIndex = blockIndex, }.ToBsonDocument()
            );
        }
    }

    public async Task<long> GetLatestBlockIndex(string? id = "SyncContext")
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
        var doc = await MetadataCollection.FindSync(filter).FirstAsync();
        return doc.GetValue("LatestBlockIndex").AsInt64;
    }

    public async Task UpsertStateDataAsyncWithLinkAvatar(
        StateData stateData,
        Address? avatarAddress = null
    )
    {
        if (
            CollectionNames.CollectionMappings.TryGetValue(
                stateData.State.GetType(),
                out var collectionName
            )
        )
        {
            var upsertResult = await UpsertStateDataAsync(stateData, collectionName);

            if (
                upsertResult != null
                && upsertResult.IsAcknowledged
                && upsertResult.UpsertedId != null
            )
            {
                var stateDataObjectId = upsertResult.UpsertedId;

                if (
                    CollectionNames.CollectionMappings.TryGetValue(
                        typeof(AvatarState),
                        out var avatarCollectionName
                    )
                )
                {
                    var avatarCollection = GetStateCollection(avatarCollectionName);

                    var address = avatarAddress?.ToHex() ?? stateData.Address.ToHex();
                    var avatarFilter = Builders<BsonDocument>.Filter.Eq("Address", address);

                    var update = Builders<BsonDocument>.Update.Set(
                        $"{collectionName.ToPascalCase()}ObjectId",
                        stateDataObjectId
                    );
                    await avatarCollection.UpdateOneAsync(avatarFilter, update);
                }
            }
        }
    }

    public async Task<ReplaceOneResult> UpsertStateDataAsync(StateData stateData)
    {
        if (
            CollectionNames.CollectionMappings.TryGetValue(
                stateData.State.GetType(),
                out var collectionName
            )
        )
        {
            return await UpsertStateDataAsync(stateData, collectionName);
        }

        throw new InvalidOperationException(
            $"No collection mapping found for state type: {stateData.State.GetType().Name}"
        );
    }

    public async Task<ReplaceOneResult> UpsertStateDataAsync(
        StateData stateData,
        string collectionName
    )
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Address", stateData.Address.ToHex());
            var bsonDocument = BsonDocument.Parse(stateData.ToJson());
            var result = await GetStateCollection(collectionName)
                .ReplaceOneAsync(filter, bsonDocument, new ReplaceOptions { IsUpsert = true });

            _logger.LogInformation(
                $"Address: {stateData.Address.ToHex()} - Stored at {collectionName}"
            );
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred during UpsertAvatarDataAsync: {ex.Message}");
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

        if (
            CollectionNames.CollectionMappings.TryGetValue(
                typeof(SheetState),
                out var tableSheetCollectionName
            )
        )
        {
            var tableSheetCollection = GetStateCollection(tableSheetCollectionName);
            await tableSheetCollection.ReplaceOneAsync(
                filter,
                document,
                new ReplaceOptions { IsUpsert = true }
            );
        }
        else
        {
            throw new InvalidOperationException(
                $"No collection mapping found for state type: {stateData.State.GetType().Name}"
            );
        }
    }
}
