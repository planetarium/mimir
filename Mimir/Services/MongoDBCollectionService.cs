using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
        var settings = MongoClientSettings.FromUrl(new MongoUrl(databaseOption.Value.ConnectionString));

        if (databaseOption.Value.CAFile is not null)
        {
            var caCertificate = new X509Certificate2(databaseOption.Value.CAFile);
            settings.AllowInsecureTls = true;
            settings.SslSettings = new SslSettings() { ClientCertificates = [caCertificate] };
        }
        var client = new MongoClient(settings);
        return client.GetDatabase(databaseOption.Value.Database);
    }
}
