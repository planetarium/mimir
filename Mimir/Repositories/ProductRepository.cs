using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Factories;
using Lib9c.Models.Market;
using Mimir.Enums;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Mimir.Repositories;

public class ProductRepository(MongoDbService dbService)
{
    private static readonly Codec Codec = new();

    public List<Product> GetProducts(long skip, int limit)
    {
        var collectionName = CollectionNames.GetCollectionName<ProductDocument>();
        return GetProducts(
            dbService.GetCollection<ProductDocument>(collectionName),
            skip,
            limit
        );
    }

    private List<Product> GetProducts(
        IMongoCollection<ProductDocument> collection,
        long skip,
        int limit)
    {
        var pipelines = new List<BsonDocument> { new("$skip", skip), new("$limit", limit) };
        var aggregation = collection.Aggregate<BsonDocument>(pipelines).ToList();
        var products = aggregation
            .Select(async doc =>
                await GetRawState(dbService.GetGridFs(), doc["RawStateFileId"].AsObjectId))
            .Select(doc => doc.Result)
            .Where(doc => doc != null)
            .Select(DeserializeProduct)
            .ToList();

        return products;
    }

    private async Task<byte[]> GetRawState(GridFSBucket gridFs, ObjectId rawStateFileId)
    {
        return await MongoDbService.RetrieveFromGridFs(gridFs, rawStateFileId);
    }

    public Product DeserializeProduct(byte[] rawState)
    {
        IValue ivalue = Codec.Decode(rawState);

        if (ivalue is not List list)
        {
            throw new InvalidCastException(nameof(Product));
        }

        var product = ProductFactory.DeserializeProduct(list);
        return product;
    }
}
