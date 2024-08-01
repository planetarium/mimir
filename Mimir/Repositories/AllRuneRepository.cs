using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.Exceptions;
using Mimir.Models.Assets;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class AllRuneRepository(MongoDbService dbService)
{
    public List<Rune> GetRunes(Address avatarAddress)
    {
        var collection = dbService.GetCollection<BsonDocument>(CollectionNames.AllRune.Value);
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
            var runesDoc = document["Object"]["Runes"].AsBsonDocument;
            return runesDoc
                .Select(rune => rune.Value.AsBsonDocument)
                .Select(runeValue => new Rune(runeValue))
                .ToList();
        }
        catch (KeyNotFoundException e)
        {
            throw new UnexpectedTypeOfBsonValueException(
                "document[\"State\"][\"Object\"][\"Runes\"].AsBsonDocument",
                e);
        }
    }
}
