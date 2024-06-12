using Lib9c.GraphQL.Enums;
using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public abstract class BaseRepository<T>
{
    private readonly MongoDBCollectionService _mongoDBCollectionService;
    private readonly Dictionary<string, IMongoCollection<T>> _collections = new();
    private readonly Dictionary<string, IMongoDatabase> _databases = new();

    protected BaseRepository(MongoDBCollectionService mongoDBCollectionService)
    {
        _mongoDBCollectionService = mongoDBCollectionService;

        var networks = new[] { "heimdall", "odin" };
        var collectionName = GetCollectionName();

        foreach (var network in networks)
        {
            string databaseName = network switch
            {
                "heimdall" => "heimdall",
                "odin" => "odin",
                _ => throw new ArgumentException("Invalid network name", nameof(network))
            };

            var collection = _mongoDBCollectionService.GetCollection<T>(
                collectionName,
                databaseName
            );
            _collections[network] = collection;

            var database = _mongoDBCollectionService.GetDatabase(databaseName);
            _databases[network] = database;
        }
    }

    protected abstract string GetCollectionName();

    protected IMongoCollection<T> GetCollection(string network)
    {
        network = network.ToLowerInvariant();
        if (_collections.TryGetValue(network, out var collection))
        {
            return collection;
        }

        throw new ArgumentException("Invalid network name", nameof(network));
    }

    protected IMongoCollection<T> GetCollection(PlanetName planetName)
    {
        var network = GetNetworkName(planetName);
        if (_collections.TryGetValue(network, out var collection))
        {
            return collection;
        }

        throw new ArgumentException("Invalid network name", nameof(network));
    }

    protected IMongoDatabase GetDatabase(string network)
    {
        network = network.ToLowerInvariant();
        if (_databases.TryGetValue(network, out var database))
        {
            return database;
        }

        throw new ArgumentException("Invalid network name", nameof(network));
    }

    private string GetNetworkName(PlanetName planetName)
    {
        return planetName switch
        {
            PlanetName.Heimdall => "heimdall",
            PlanetName.Odin => "odin",
            _ => throw new ArgumentException("Invalid network name", nameof(planetName))
        };
    }
}
