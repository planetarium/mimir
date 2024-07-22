using Libplanet.Crypto;
using Mimir.Exceptions;
using Mimir.GraphQL.Extensions;
using Mimir.Models.AdventureBoss;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Helper;
using Nekoyume.Module;

namespace Mimir.Repositories.AdventureBoss;

public class SeasonInfoRepository(MongoDbService dbService)
{
    public SeasonInfo GetSeasonInfo(long number)
    {
        var address = new Address(AdventureBossHelper.GetSeasonAsAddressForm(number));
        var collection = dbService.GetCollection<BsonDocument>("adventure_boss_season_info");
        var filter = Builders<BsonDocument>.Filter.Eq("Address", address.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'Address' equals to '{address.ToHex()}'");
        }

        try
        {
            var doc = document["State"]["Object"].AsBsonDocument;
            return new SeasonInfo(
                new Address(doc["Address"].AsString),
                doc["Season"].ToLong(),
                doc["StartBlockIndex"].ToLong(),
                doc["EndBlockIndex"].ToLong(),
                doc["NextStartBlockIndex"].ToLong(),
                doc["BossId"].AsInt32);
        }
        catch (KeyNotFoundException e)
        {
            throw new UnexpectedTypeOfBsonValueException(
                "document[\"State\"][\"Object\"].AsBsonDocument",
                e);
        }
    }
}
