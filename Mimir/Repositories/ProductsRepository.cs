using Lib9c.GraphQL.Enums;
using Mimir.Models.Product;
using Mimir.Services;

namespace Mimir.Repositories;

public class ProductsRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<Product>(mongoDbCollectionService)
{
    public IExecutable<Product> GetProducts(PlanetName planetName)
    {
        var collection = GetCollection<Product>(planetName);
        return collection.AsExecutable();
    }

    protected override string GetCollectionName() => "product";
}
