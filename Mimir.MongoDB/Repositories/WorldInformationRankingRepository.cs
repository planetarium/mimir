using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.MongoDb;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public interface IWorldInformationRankingRepository
{
    IExecutable<WorldInformationDocument> GetRanking();
    Task<int?> GetMyRanking(string address);
    Task<(WorldInformationDocument? UserDocument, int Rank)?> GetUserWithRanking(string address);
}

public class WorldInformationRankingRepository : IWorldInformationRankingRepository
{
    private readonly IMongoCollection<WorldInformationDocument> _collection;

    public WorldInformationRankingRepository(IMongoDbService dbService)
    {
        var collectionName = CollectionNames.GetCollectionName<WorldInformationDocument>();
        _collection = dbService.GetCollection<WorldInformationDocument>(collectionName);
    }

    public IExecutable<WorldInformationDocument> GetRanking()
    {
        var find = _collection.Find(Builders<WorldInformationDocument>.Filter.Empty);
        
        var sortBuilder = Builders<WorldInformationDocument>.Sort;
        var sortDefinition = sortBuilder.Descending("LastStageClearedId");

        return find.Sort(sortDefinition).AsExecutable();
    }
    
    public async Task<int?> GetMyRanking(string address)
    {
        var userDocument = await _collection.Find(Builders<WorldInformationDocument>.Filter.Eq("_id", address)).FirstOrDefaultAsync();
        
        if (userDocument == null)
            return null;

        var userStageCleared = userDocument.LastStageClearedId;
        
        // Count documents with higher stage cleared than the user's
        var higherStageCount = await _collection.CountDocumentsAsync(
            Builders<WorldInformationDocument>.Filter.Gt("LastStageClearedId", userStageCleared));
            
        // Add 1 to get the rank (1-based index)
        return (int)higherStageCount + 1;
    }
    
    public async Task<(WorldInformationDocument? UserDocument, int Rank)?> GetUserWithRanking(string address)
    {
        var userDocument = await _collection.Find(Builders<WorldInformationDocument>.Filter.Eq("_id", address)).FirstOrDefaultAsync();
        
        if (userDocument == null)
            return null;

        var userStageCleared = userDocument.LastStageClearedId;
        
        // Count documents with higher stage cleared than the user's
        var higherStageCount = await _collection.CountDocumentsAsync(
            Builders<WorldInformationDocument>.Filter.Gt("LastStageClearedId", userStageCleared));
            
        // Add 1 to get the rank (1-based index)
        int rank = (int)higherStageCount + 1;
        
        return (userDocument, rank);
    }
} 