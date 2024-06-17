using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.Models;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class AvatarRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    public Avatar GetAvatar(string network, Address avatarAddress) =>
        GetAvatar(GetCollection(network), avatarAddress);

    public Avatar GetAvatar(PlanetName planetName, Address avatarAddress) =>
        GetAvatar(GetCollection(planetName), avatarAddress);

    private static Avatar GetAvatar(
        IMongoCollection<BsonDocument> collection,
        Address avatarAddress)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{avatarAddress.ToHex()}'");
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
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundInBsonDocumentException("document[\"State\"]", e);
        }
    }

    protected override string GetCollectionName() => "avatar";
}
