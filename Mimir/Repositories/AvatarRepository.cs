using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.Models;
using Mimir.MongoDB.Exceptions;
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
            var avatarDoc = document["Object"].AsBsonDocument;
            return new Avatar(
                avatarDoc["AgentAddress"].AsString,
                avatarDoc["Address"].AsString,
                avatarDoc["Name"].AsString,
                avatarDoc["Level"].AsInt32,
                avatarDoc["ActionPoint"].AsInt32,
                avatarDoc["DailyRewardReceivedIndex"].ToInt64(),
                avatarDoc["CharacterId"].ToInt32()
            );
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException("document[\"State\"][\"Object\"]", e);
        }
        catch (InvalidCastException e)
        {
            throw new UnexpectedTypeOfBsonValueException(
                "document[\"State\"][\"Object\"].AsBsonDocument or its children values",
                e);
        }
    }
}
