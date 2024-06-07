using Lib9c.GraphQL.Enums;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public abstract class DiffRepository
{
    private readonly Dictionary<PlanetName, IMongoCollection<BsonDocument>> _collections = new();
    private readonly Dictionary<PlanetName, IMongoDatabase> _databases = new();

    protected DiffRepository(
        MongoDBCollectionService mongoDbCollectionService,
        string collectionName)
    {
        foreach (var planetName in new[] { PlanetName.Heimdall, PlanetName.Odin })
        {
            var databaseName = PlanetNameToDatabaseName(planetName);
            var collection = mongoDbCollectionService.GetCollection<BsonDocument>(
                collectionName,
                databaseName);
            _collections[planetName] = collection;

            var database = mongoDbCollectionService.GetDatabase(databaseName);
            _databases[planetName] = database;
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

    protected IMongoCollection<BsonDocument> GetCollection(PlanetName planetName)
    {
        if (!_collections.TryGetValue(planetName, out var collection))
        {
            throw new ArgumentException(
                $"Invalid network name. Expected one of {string.Join(", ", _collections.Keys)} but got {planetName}",
                nameof(planetName));
        }

        return collection;
    }

    protected IMongoDatabase GetDatabase(PlanetName planetName)
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
