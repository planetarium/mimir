using Mimir.Services;
using MongoDB.Driver;

namespace Mimir.Repositories;

public abstract class BaseRepository<T>
{
    private readonly MongoDBCollectionService _mongoDBCollectionService;
    private readonly IMongoCollection<T> _collection;
    private readonly IMongoDatabase _database;

    protected BaseRepository(MongoDBCollectionService mongoDBCollectionService)
    {
        _mongoDBCollectionService = mongoDBCollectionService;
        _database = _mongoDBCollectionService.GetDatabase();
        _collection = _database.GetCollection<T>(
            GetCollectionName()
        );
    }

    protected abstract string GetCollectionName();

    protected IMongoCollection<T> GetCollection() => _collection;
    protected IMongoDatabase GetDatabase() => _database;
}
