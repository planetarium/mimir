using Bencodex;
using HotChocolate;
using HotChocolate.Data;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Mimir.MongoDB.Repositories;

public class ProductRepository
{
    private static readonly Codec Codec = new();

    private readonly IMongoCollection<ProductDocument> _collection;
    private readonly GridFSBucket _gridFsBucket;

    public ProductRepository(MongoDbService dbService)
    {
        var collectionName = CollectionNames.GetCollectionName<ProductDocument>();
        _collection = dbService.GetCollection<ProductDocument>(collectionName);
        _gridFsBucket = dbService.GetGridFs();
    }

    public IExecutable<ProductDocument> GetAll() => _collection
        .Find(Builders<ProductDocument>.Filter.Empty)
        .AsExecutable();

    public async Task<ProductDocument> GetByProductIdAsync(Guid productId)
    {
        var filter = Builders<ProductDocument>.Filter.Eq(doc => doc.Object.ProductId, productId);
        var productDocument = await _collection.Find(filter).FirstOrDefaultAsync();
        if (productDocument is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                _collection.CollectionNamespace.CollectionName,
                $"'ProductId' equals to '{productId}'");
        }

        return productDocument;
    }
}
