using HotChocolate;
using HotChocolate.Data;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public interface ICpRepository<T> where T : ICpDocument
{
    IExecutable<T> GetRanking();
    Task<int?> GetMyRanking(string address);
    Task<(T? UserDocument, int Rank)?> GetUserWithRanking(string address);
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
        var sortDefinition = Builders<T>.Sort.Descending("Cp");

        return find.Sort(sortDefinition).AsExecutable();
    }

    public async Task<int?> GetMyRanking(string address)
    {
        var userDocument = await _collection.Find(Builders<T>.Filter.Eq("_id", address)).FirstOrDefaultAsync();
        
        if (userDocument == null)
            return null;

        var userCp = userDocument.Cp;
        
        // Count documents with higher CP than the user's
        var higherCpCount = await _collection.CountDocumentsAsync(
            Builders<T>.Filter.Gt("Cp", userCp));
            
        // Add 1 to get the rank (1-based index)
        return (int)higherCpCount + 1;
    }
    
    public async Task<(T? UserDocument, int Rank)?> GetUserWithRanking(string address)
    {
        var userDocument = await _collection.Find(Builders<T>.Filter.Eq("_id", address)).FirstOrDefaultAsync();
        
        if (userDocument == null)
            return null;

        var userCp = userDocument.Cp;
        
        // Count documents with higher CP than the user's
        var higherCpCount = await _collection.CountDocumentsAsync(
            Builders<T>.Filter.Gt("Cp", userCp));
            
        // Add 1 to get the rank (1-based index)
        int rank = (int)higherCpCount + 1;
        
        return (userDocument, rank);
    }
}