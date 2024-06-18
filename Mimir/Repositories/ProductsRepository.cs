using Lib9c.GraphQL.Enums;
using Mimir.Models.Product;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class ProductsRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<Product>(mongoDbCollectionService)
{
    public IQueryable<Product> GetProducts(PlanetName planetName)
    {
        var collection = GetCollection<Product>(planetName);
        return collection.AsQueryable();
    }

    protected override string GetCollectionName() => "product";
}
