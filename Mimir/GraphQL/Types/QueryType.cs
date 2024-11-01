using Mimir.GraphQL.Queries;
using Mimir.GraphQL.Types.MimirBsonDocuments;
using Mimir.MongoDB.Repositories;

namespace Mimir.GraphQL.Types;

public class QueryType : ObjectType<Query>
{
    
    public class ProductFilterInputType : InputObjectType<ProductRepository.ProductFilter>;

    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field("products")
            .Description("Retrieves a list of market products.")
            .Argument("filter", a => a.Type<ProductFilterInputType>())
            .UseOffsetPaging<ProductDocumentType>()
            .Resolve(context =>
            {
                var productFilter = context.ArgumentValue<ProductRepository.ProductFilter?>("filter");
                return context.Service<ProductRepository>().Get(productFilter);
            });
    }
}
