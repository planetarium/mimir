// using Libplanet.Crypto;
// using Lib9c.Models.AdventureBoss;
// using Mimir.Exceptions;
// using Mimir.MongoDB.Bson.Extensions;
// using Mimir.MongoDB.Exceptions;
// using Mimir.MongoDB.Services;
// using MongoDB.Bson;
// using MongoDB.Driver;
// using Nekoyume.Helper;

// namespace Mimir.MongoDB.Repositories.AdventureBoss;

// public class SeasonInfoRepository(MongoDbService dbService)
// {
//     public SeasonInfo GetSeasonInfo(long number)
//     {
//         var address = new Address(AdventureBossHelper.GetSeasonAsAddressForm(number));
//         var collection = dbService.GetCollection<BsonDocument>("adventure_boss_season_info");
//         var filter = Builders<BsonDocument>.Filter.Eq("_id", address.ToHex());
//         var document = collection.Find(filter).FirstOrDefault();
//         if (document is null)
//         {
//             throw new DocumentNotFoundInMongoCollectionException(
//                 collection.CollectionNamespace.CollectionName,
//                 $"'Address' equals to '{address.ToHex()}'");
//         }

//         try
//         {
//             var doc = document["Object"].AsBsonDocument;
//             return new SeasonInfo(
//                 new Address(doc["Address"].AsString),
//                 doc["Season"].ToLong(),
//                 doc["StartBlockIndex"].ToLong(),
//                 doc["EndBlockIndex"].ToLong(),
//                 doc["NextStartBlockIndex"].ToLong(),
//                 doc["BossId"].AsInt32);
//         }
//         catch (KeyNotFoundException e)
//         {
//             throw new UnexpectedTypeOfBsonValueException(
//                 "document[\"State\"][\"Object\"].AsBsonDocument",
//                 e);
//         }
//     }
// }
