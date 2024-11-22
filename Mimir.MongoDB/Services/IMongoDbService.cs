using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Mimir.MongoDB.Services;

public interface IMongoDbService
{
    IMongoCollection<T> GetCollection<T>(string collectionName);
    IMongoDatabase GetDatabase();
    GridFSBucket GetGridFs();
    Task<byte[]> RetrieveFromGridFs(ObjectId fileId);
}
