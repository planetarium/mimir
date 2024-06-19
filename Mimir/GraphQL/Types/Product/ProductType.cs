using Lib9c.GraphQL.Types;
using Mimir.Models.Product;

namespace Mimir.GraphQL.Types;

public class ProductType : ObjectType<Product>
{
    protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor
            .Field(f => f.Address)
            .Type<NonNullType<StringType>>();
        descriptor.Field(p => p.State).Type<NonNullType<ProductStateType>>();
    }
}
