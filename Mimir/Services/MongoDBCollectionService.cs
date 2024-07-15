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
        if (databaseOption.Value.CAFile is not null)
        {
            X509Store localTrustStore = new X509Store(StoreName.Root);
            X509Certificate2Collection certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(databaseOption.Value.CAFile);
            try
            {
                localTrustStore.Open(OpenFlags.ReadWrite);
                localTrustStore.AddRange(certificateCollection);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Root certificate import failed: " + ex.Message);
                throw;
            }
            finally
            {
                localTrustStore.Close();
            }
        }

        var settings = MongoClientSettings.FromUrl(new MongoUrl(databaseOption.Value.ConnectionString));
        var client = new MongoClient(settings);
        return client.GetDatabase(databaseOption.Value.Database);
    }
}
