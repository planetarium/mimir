using Lib9c.GraphQL.Enums;
using Mimir.Models.Product;
using Mimir.Repositories;

namespace Mimir.GraphQL.Queries;

public class Query
{
    public IQueryable<Product> GetProducts(
        [Service] ProductsRepository productsRepository,
        PlanetName planetName
    )
    {
        return productsRepository.GetProducts(planetName);
    }
}
