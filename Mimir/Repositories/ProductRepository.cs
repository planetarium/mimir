using Bencodex;
using Bencodex.Types;
using Mimir.Models.Factories;
using Mimir.Models.Market;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class ProductRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    protected override string GetCollectionName() => "product";

    private static readonly Codec Codec = new();

    public List<Product> GetProducts(long skip, int limit) =>
        GetProducts(GetCollection(), skip, limit);

    private static List<Product> GetProducts(
        IMongoCollection<BsonDocument> collection,
        long skip,
        int limit
    )
    {
        var pipelines = new List<BsonDocument> { new("$skip", skip), new("$limit", limit) };

        var aggregation = collection.Aggregate<BsonDocument>(pipelines).ToList();
        List<Product> products = aggregation
            .Select(doc => DeserializeProduct(doc["State"]["Raw"].AsString))
            .ToList();

        return products;
    }

    public static Product DeserializeProduct(string rawState)
    {
        IValue ivalue = Codec.Decode(Convert.FromHexString(rawState));

        if (ivalue is not List list)
        {
            throw new InvalidCastException(nameof(Product));
        }

        var product = ProductFactory.DeserializeProduct(list);
        return product;
    }
}
