using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;

namespace Mimir.MongoDB.Repositories;

public interface IActionTypeRepository
{
    Task<IEnumerable<ActionTypeDocument>> GetAllAsync();
}

public class ActionTypeRepository(IMongoDbService dbService) : IActionTypeRepository
{
    public virtual async Task<IEnumerable<ActionTypeDocument>> GetAllAsync()
    {
        var collectionName = CollectionNames.GetCollectionName<ActionTypeDocument>();
        var collection = dbService.GetCollection<ActionTypeDocument>(collectionName);
        var documents = await collection.Find(_ => true).ToListAsync();
        return documents;
    }
} 