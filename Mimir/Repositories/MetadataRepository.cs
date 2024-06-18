using Mimir.Exceptions;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class MetadataRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    protected override string GetCollectionName() => "metadata";

    public async Task<long> GetLatestBlockIndex(string network, string pollerType)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("PollerType", pollerType);
        var doc = await GetCollection(network).FindSync(filter).FirstAsync();
        try
        {
            return doc["LatestBlockIndex"].AsInt64;
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException(
                "doc[\"LatestBlockIndex\"].AsInt64",
                e);
        }
    }
}
