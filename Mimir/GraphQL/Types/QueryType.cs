using Mimir.GraphQL.Queries;
using Mimir.GraphQL.Types.MimirBsonDocuments;
using Mimir.Repositories;

namespace Mimir.GraphQL.Types;

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field("products")
            .Description("Retrieves a list of market products.")
            .UseOffsetPaging<ProductDocumentType>()
            .Resolve(context => context.Service<ProductRepository>().GetAll());
    }
}
