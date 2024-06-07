using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Models.Assets;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class AllRuneRepository
{
    private readonly MongoDBCollectionService _mongoDBCollectionService;
    private readonly Dictionary<PlanetName, IMongoCollection<BsonDocument>> _collections = new();
    private readonly Dictionary<PlanetName, IMongoDatabase> _databases = new();

    public AllRuneRepository(MongoDBCollectionService mongoDbCollectionService)
    {
        _mongoDBCollectionService = mongoDbCollectionService;

        const string collectionName = "all_rune";
        foreach (var planetName in new[] { PlanetName.Heimdall, PlanetName.Odin })
        {
            var databaseName = PlanetNameToDatabaseName(planetName);
            var collection = _mongoDBCollectionService.GetCollection<BsonDocument>(
                collectionName,
                databaseName);
            _collections[planetName] = collection;

            var database = _mongoDBCollectionService.GetDatabase(databaseName);
            _databases[planetName] = database;
        }
    }

    public List<Rune>? GetRunes(PlanetName planetName, Address avatarAddress)
    {
        var collection = GetCollection(planetName);
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

    private static string PlanetNameToDatabaseName(PlanetName planetName)
    {
        return planetName switch
        {
            PlanetName.Heimdall => "heimdall_diff_test",
            PlanetName.Odin => "odin_diff_test",
            _ => throw new ArgumentException("Invalid planet name", nameof(planetName))
        };
    }

    private IMongoCollection<BsonDocument> GetCollection(PlanetName planetName)
    {
        if (!_collections.TryGetValue(planetName, out var collection))
        {
            throw new ArgumentException(
                $"Invalid planet name. Expected one of {string.Join(", ", _collections.Keys)} but got {planetName}",
                nameof(planetName));
        }

        return collection;
    }

    private IMongoDatabase GetDatabase(PlanetName planetName)
    {
        if (!_databases.TryGetValue(planetName, out var database))
        {
            throw new ArgumentException(
                $"Invalid network name. Expected one of {string.Join(", ", _databases.Keys)} but got {planetName}",
                nameof(planetName));
        }

        return database;
    }
}
