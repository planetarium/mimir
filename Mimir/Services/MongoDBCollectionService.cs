using Microsoft.Extensions.Options;
using Mimir.Options;
using MongoDB.Driver;

namespace Mimir.Services;

public class MongoDBCollectionService(IOptions<DatabaseOption> databaseOption)
{
    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        var database = GetDatabase();
        return database.GetCollection<T>(collectionName);
    }

    public IMongoDatabase GetDatabase()
    {
        var client = new MongoClient(databaseOption.Value.ConnectionString);
        return client.GetDatabase(databaseOption.Value.Database);
    }
}
