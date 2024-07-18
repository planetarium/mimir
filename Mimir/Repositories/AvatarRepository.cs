using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.Models;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class AvatarRepository(MongoDbService dbService)
{
    public Avatar GetAvatar(Address avatarAddress) =>
        GetAvatar(
            dbService.GetCollection<BsonDocument>(CollectionNames.Avatar.Value),
            avatarAddress
        );

    public static Avatar GetAvatar(
        IMongoCollection<BsonDocument> collection,
        Address avatarAddress
    )
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{avatarAddress.ToHex()}'"
            );
        }

        try
        {
            var avatarDoc = document["State"]["Object"];
            return new Avatar(
                avatarDoc["agentAddress"].AsString,
                avatarDoc["address"].AsString,
                avatarDoc["name"].AsString,
                avatarDoc["level"].AsInt32,
                avatarDoc["actionPoint"].AsInt32,
                avatarDoc["dailyRewardReceivedIndex"].ToInt64(),
                avatarDoc["characterId"].ToInt32()
            );
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException("document[\"State\"]", e);
        }
    }
}
