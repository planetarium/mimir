using System.Text;
using Libplanet.Crypto;
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

    private IMongoCollection<BsonDocument> ArenaCollection =>
        _database.GetCollection<BsonDocument>("arena");

    private IMongoCollection<BsonDocument> AvatarCollection =>
        _database.GetCollection<BsonDocument>("avatars");

    private IMongoCollection<BsonDocument> MetadataCollection =>
        _database.GetCollection<BsonDocument>("metadata");

    private IMongoCollection<BsonDocument> TableSheetsCollection =>
        _database.GetCollection<BsonDocument>("tableSheets");

    public DiffMongoDbService(ILogger<DiffMongoDbService> logger, string connectionString, string databaseName)
    {
        _client = new MongoClient(connectionString);
        _database = _client.GetDatabase(databaseName);
        _logger = logger;
        _databaseName = databaseName;
        _gridFs = new GridFSBucket(_database);
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
                new SyncContext
                {
                    LatestBlockIndex = blockIndex,
                }.ToBsonDocument());
        }
    }

    public async Task<long> GetLatestBlockIndex()
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", "SyncContext");
        var doc = await MetadataCollection.FindSync(filter).FirstAsync();
        return doc.GetValue("LatestBlockIndex").AsInt64;
    }

    public async Task UpsertAvatarDataAsync(OnlyAvatarData avatarData)
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq(
                "State.Avatar.address",
                avatarData.State.address.ToHex()
            );
            var bsonDocument = BsonDocument.Parse(avatarData.ToJson());
            await AvatarCollection.ReplaceOneAsync(
                filter,
                bsonDocument,
                new ReplaceOptions { IsUpsert = true }
            );

            _logger.LogInformation($"Stored avatar data");
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred during UpsertAvatarDataAsync: {ex.Message}");
        }
    }

    public async Task<bool> IsInitialized()
    {
        var names = await (
            await _client.GetDatabase(_databaseName).ListCollectionNamesAsync()
        ).ToListAsync();
        return names is not { } ns || !(ns.Contains("arena") && ns.Contains("avatars"));
    }
}
