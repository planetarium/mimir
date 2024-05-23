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

    private readonly string _databaseName;

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
        _databaseName = databaseName;
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

    private IMongoCollection<BsonDocument> GetStateCollection(string collectionName)
    {
        return _stateCollectionMappings[collectionName];
    }

    public async Task UpdateLatestBlockIndex(long blockIndex)
    {
        _logger.LogInformation($"Update latest block index to {blockIndex}");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", "SyncContext");
        var update = Builders<BsonDocument>.Update.Set("LatestBlockIndex", blockIndex);

        var response = await MetadataCollection.UpdateOneAsync(filter, update);
        if (response?.ModifiedCount < 1)
        {
            await MetadataCollection.InsertOneAsync(
                new SyncContext { LatestBlockIndex = blockIndex, }.ToBsonDocument()
            );
        }
    }

    public async Task<long> GetLatestBlockIndex()
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", "SyncContext");
        var doc = await MetadataCollection.FindSync(filter).FirstAsync();
        return doc.GetValue("LatestBlockIndex").AsInt64;
    }

    public async Task UpsertStateDataAsync(StateData stateData, string collectionName)
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Address", stateData.Address.ToHex());
            var bsonDocument = BsonDocument.Parse(stateData.ToJson());
            await GetStateCollection(collectionName)
                .ReplaceOneAsync(filter, bsonDocument, new ReplaceOptions { IsUpsert = true });

            _logger.LogInformation($"Stored at {collectionName}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred during UpsertAvatarDataAsync: {ex.Message}");
        }
    }
}
