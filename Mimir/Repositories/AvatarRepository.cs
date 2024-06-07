using Libplanet.Crypto;
using Mimir.Models;
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
        var filter = Builders<BsonDocument>.Filter.Eq("Avatar.address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            return null;
        }

        try
        {
            var avatarDoc = document["Avatar"];
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

    public Inventory? GetInventory(string network, Address avatarAddress)
    {
        var collection = GetCollection(network);
        var filter = Builders<BsonDocument>.Filter.Eq("Avatar.address", avatarAddress.ToHex());
        var projection = Builders<BsonDocument>.Projection.Include("Avatar.inventory");
        var document = collection.Find(filter).Project(projection).FirstOrDefault();
        if (document is null)
        {
            return null;
        }

        try
        {
            return new Inventory(document["Avatar"]["inventory"].AsBsonDocument);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
}
