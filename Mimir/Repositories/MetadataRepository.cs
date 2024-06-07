using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class MetadataRepository : BaseRepository<BsonDocument>
{
    public MetadataRepository(MongoDBCollectionService mongoDBCollectionService)
        : base(mongoDBCollectionService)
    {
    }

    protected override string GetCollectionName()
    {
        return "metadata";
    }

    public async Task<long> GetLatestBlockIndex(string network)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", "SyncContext");
        var doc = await GetCollection(network).FindSync(filter).FirstAsync();
        return doc.GetValue("LatestBlockIndex").AsInt64;
    }
}
