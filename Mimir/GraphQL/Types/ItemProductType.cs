using Lib9c.GraphQL.Types;
using Lib9c.Models.Market;

namespace Mimir.GraphQL.Types;

public class ItemProductType : ObjectType<ItemProduct>
{
    protected override void Configure(IObjectTypeDescriptor<ItemProduct> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor.Field(f => f.ProductId).Type<UuidType>();
        descriptor.Field(f => f.SellerAgentAddress).Type<AddressType>();
        descriptor.Field(f => f.SellerAvatarAddress).Type<AddressType>();
        descriptor.Field(f => f.Price).Type<FungibleAssetValueType>();
        descriptor.Field(f => f.RegisteredBlockIndex).Type<LongType>();
        descriptor.Field(f => f.ItemCount).Type<IntType>();
        descriptor.Field(f => f.TradableItem).Type<TradableItemType>();
    }
}
