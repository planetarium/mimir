using HotChocolate.Types;
using Lib9c.GraphQL.Types.Items;
using Lib9c.Models.Market;

namespace Lib9c.GraphQL.Types.Market;

public class ItemProductType : ObjectType<ItemProduct>
{
    protected override void Configure(IObjectTypeDescriptor<ItemProduct> descriptor)
    {
        descriptor.Implements<ProductInterfaceType>();
        descriptor
            .Field(f => f.TradableItem)
            .Type<ItemBaseInterfaceType>();
    }
}
