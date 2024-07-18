using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Options;
using Mimir.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Mimir.Services;

public class MongoDbService
{
    private IMongoDatabase database;
    private GridFSBucket gridFs;

    public MongoDbService(IOptions<DatabaseOption> databaseOption)
    {
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
        database = client.GetDatabase(databaseOption.Value.Database);
        gridFs = new GridFSBucket(database);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        var database = GetDatabase();
        return database.GetCollection<T>(collectionName);
    }

    public IMongoDatabase GetDatabase()
    {
        return database;
    }

    public GridFSBucket GetGridFs()
    {
        return gridFs;
    }

    public async Task<byte[]> RetrieveFromGridFs(ObjectId fileId)
    {
        return await RetrieveFromGridFs(gridFs, fileId);
    }

    public static async Task<byte[]> RetrieveFromGridFs(GridFSBucket gridFs, ObjectId fileId)
    {
        var fileBytes = await gridFs.DownloadAsBytesAsync(fileId);
        return fileBytes;
    }
}
