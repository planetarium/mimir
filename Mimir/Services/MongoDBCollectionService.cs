using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Mimir.Options;

namespace Mimir.Services;

public class MongoDBCollectionService(IOptions<DatabaseOption> databaseOption)
{
    private readonly IOptions<DatabaseOption> _databaseOption = databaseOption;

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        var client = new MongoClient(_databaseOption.Value.ConnectionString);
        var database = client.GetDatabase(_databaseOption.Value.DatabaseName);
        return database.GetCollection<T>(collectionName);
    }
}
