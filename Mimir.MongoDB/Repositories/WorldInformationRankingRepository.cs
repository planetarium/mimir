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
    Task<int> GetRankByAddressAsync(Address address);
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
        
        // var sortBuilder = Builders<WorldInformationDocument>.Sort;
        // var sortDefinition = sortBuilder.Descending("Object.WorldDictionary.$[-1].StageClearedId");

        return find.AsExecutable();
    }

    public async Task<int> GetRankByAddressAsync(Address address)
    {
        var target = await _collection.Find(
            Builders<WorldInformationDocument>.Filter.Eq("_id", address.ToHex())
        ).FirstOrDefaultAsync();

        if (target == null)
        {
            return -1;
        }

        // Find the max world ID
        var maxWorldId = target.Object.WorldDictionary.Keys.Max();
        var stageClearedId = target.Object.WorldDictionary[maxWorldId].StageClearedId;

        // Count documents with higher StageClearedId
        var countHigher = await _collection.CountDocumentsAsync(
            Builders<WorldInformationDocument>.Filter.Gt(
                "Object.WorldDictionary.$[-1].StageClearedId", 
                stageClearedId
            )
        );

        return (int)countHigher + 1; // Adding 1 to convert from 0-based to 1-based ranking
    }
} 