using Microsoft.Extensions.Options;
using Mimir.Options;
using MongoDB.Driver;

namespace Mimir.Services;

public class MongoDBCollectionService(IOptions<DatabaseOption> databaseOption)
{
    private readonly IOptions<DatabaseOption> _databaseOption = databaseOption;

    public IMongoCollection<T> GetCollection<T>(string collectionName, string databaseName)
    {
        var database = GetDatabase(databaseName);
        return database.GetCollection<T>(collectionName);
    }

    public IMongoDatabase GetDatabase(string databaseName)
    {
        var client = new MongoClient(_databaseOption.Value.ConnectionString);
        return client.GetDatabase(databaseName);
    }
}
