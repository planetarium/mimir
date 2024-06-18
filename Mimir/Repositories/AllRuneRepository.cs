using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Models.Assets;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class AllRuneRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    public List<Rune>? GetRunes(PlanetName planetName, Address avatarAddress)
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
            var runesDoc = document["State"]["Object"]["Runes"].AsBsonDocument;
            return runesDoc
                .Select(rune => rune.Value.AsBsonDocument)
                .Select(runeValue => new Rune(runeValue))
                .ToList();
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    protected override string GetCollectionName() => "all_rune";
}
