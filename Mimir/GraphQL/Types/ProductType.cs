using Mimir.Models.Market;

namespace Mimir.GraphQL.Types;

public class ProductType : UnionType<Product>
{
    protected override void Configure(IUnionTypeDescriptor descriptor)
    {
        descriptor.Type<FAVProductType>();
        descriptor.Type<ItemProductType>();
    }
}
