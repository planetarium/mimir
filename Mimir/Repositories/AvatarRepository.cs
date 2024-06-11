using Libplanet.Crypto;
using Mimir.Models.Agent;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class AvatarRepository : BaseRepository<BsonDocument>
{
    public AvatarRepository(MongoDBCollectionService mongoDBCollectionService)
        : base(mongoDBCollectionService)
    {
    }

    protected override string GetCollectionName()
    {
        return "avatar";
    }
    
    public Avatar? GetAvatar(string network, Address avatarAddress)
    {
        var collection = GetCollection(network);
        var filter = Builders<BsonDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            return null;
        }

        try
        {
            var avatarDoc = document["State"];
            return new Avatar(
                avatarDoc["agentAddress"].AsString,
                avatarDoc["address"].AsString,
                avatarDoc["name"].AsString,
                avatarDoc["level"].AsInt32,
                avatarDoc["actionPoint"].AsInt32,
                avatarDoc["dailyRewardReceivedIndex"].ToInt64()
            );
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
}
