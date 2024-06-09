using Libplanet.Crypto;
using Mimir.Models.Agent;
using Mimir.Models.Avatar;
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
        return "avatars";
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
            var avatarDoc = document["Avatar"];
            return new Avatar(
                document["State"]["Object"]["agentAddress"].AsString,
                document["State"]["Object"]["address"].AsString,
                document["State"]["Object"]["name"].AsString,
                document["State"]["Object"]["level"].AsInt32,
                document["State"]["Object"]["actionPoint"].AsInt32,
                document["State"]["Object"]["dailyRewardReceivedIndex"].ToInt64()
            );
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
}
