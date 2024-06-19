using HotChocolate.Types;
using Mimir.Models.Product;

namespace Mimir.GraphQL.Types;

public class ItemProductObjectType : ObjectType<ItemProductObject>
{
    protected override void Configure(IObjectTypeDescriptor<ItemProductObject> descriptor)
    {
        descriptor.Field(t => t.TradableItem).Type<TradableItemType>();
        descriptor.Field(t => t.ItemCount).Type<IntType>();
        descriptor.Field(t => t.ProductId).Type<StringType>();
        descriptor.Field(t => t.Type).Type<IntType>();
        descriptor.Field(t => t.Price).Type<PriceType>();
        descriptor.Field(t => t.RegisteredBlockIndex).Type<LongType>();
        descriptor.Field(t => t.SellerAvatarAddress).Type<StringType>();
        descriptor.Field(t => t.SellerAgentAddress).Type<StringType>();
    }
}
