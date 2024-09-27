using HotChocolate.Types;
using Lib9c.Models.Market;

namespace Lib9c.GraphQL.Types.Market;

public class ItemProductType : ObjectType<ItemProduct>
{
    protected override void Configure(IObjectTypeDescriptor<ItemProduct> descriptor)
    {
        descriptor.Implements<ProductInterfaceType>();
    }
}
