using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.MongoDb;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public interface ICpRepository<T> where T : ICpDocument
{
    IExecutable<T> GetRanking();
}

public class CpRepository<T> : ICpRepository<T> where T : ICpDocument
{
    private readonly IMongoCollection<T> _collection;

    public CpRepository(IMongoDbService dbService)
    {
        var collectionName = CollectionNames.GetCollectionName<T>();
        _collection = dbService.GetCollection<T>(collectionName);
    }

    public IExecutable<T> GetRanking()
    {
        var find = _collection.Find(Builders<T>.Filter.Empty);
        
        var sortBuilder = Builders<T>.Sort;
        var sortDefinition = sortBuilder.Descending("Cp");

        return find.Sort(sortDefinition).AsExecutable();
    }
} 