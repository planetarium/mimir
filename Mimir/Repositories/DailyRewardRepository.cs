using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class DailyRewardRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    public long? GetDailyReward(PlanetName planetName, Address avatarAddress)
    {
        var collection = GetCollection<BsonDocument>(planetName);
        var filter = Builders<BsonDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            return null;
        }

        try
        {
            var obj = document["State"]["Object"];
            return obj.BsonType switch
            {
                BsonType.Int32 => obj.AsInt32,
                BsonType.Int64 => obj.AsInt64,
                _ => null,
            };
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    protected override string GetCollectionName() => "daily_reward";
}
