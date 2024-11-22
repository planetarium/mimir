using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Bson.Serialization;
using Mimir.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Mimir.MongoDB.Services;

public class MongoDbService : IMongoDbService
{
    private readonly IMongoDatabase _database;
    private readonly GridFSBucket _gridFs;

    public MongoDbService(IOptions<DatabaseOption> databaseOption)
    {
        SerializationRegistry.Register();
        var settings = MongoClientSettings.FromUrl(
            new MongoUrl(databaseOption.Value.ConnectionString)
        );

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

        var client = new MongoClient(settings);
        _database = client.GetDatabase(databaseOption.Value.Database);
        _gridFs = new GridFSBucket(_database);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        var database = GetDatabase();
        return database.GetCollection<T>(collectionName);
    }

    public IMongoDatabase GetDatabase()
    {
        return _database;
    }

    public GridFSBucket GetGridFs()
    {
        return _gridFs;
    }

    public async Task<byte[]> RetrieveFromGridFs(ObjectId fileId)
    {
        return await RetrieveFromGridFs(_gridFs, fileId);
    }

    public static async Task<byte[]> RetrieveFromGridFs(GridFSBucket gridFs, ObjectId fileId)
    {
        var fileBytes = await gridFs.DownloadAsBytesAsync(fileId);
        return fileBytes;
    }
}
