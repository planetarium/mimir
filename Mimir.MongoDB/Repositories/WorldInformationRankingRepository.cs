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
} 