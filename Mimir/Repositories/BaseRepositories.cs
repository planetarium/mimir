using System.Collections.Generic;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public abstract class BaseRepository<T>
{
    private readonly MongoDBCollectionService _mongoDBCollectionService;
    private readonly Dictionary<string, IMongoCollection<T>> _collections = new();

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
        }
    }

    protected abstract string GetCollectionName();

    protected IMongoCollection<T> GetCollection(string network)
    {
        if (_collections.TryGetValue(network, out var collection))
        {
            return collection;
        }

        throw new ArgumentException("Invalid network name", nameof(network));
    }
}
