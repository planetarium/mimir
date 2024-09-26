using HotChocolate.Types;
using Nekoyume.Model.Item;

namespace Lib9c.GraphQL.Types.Items;

public class TradableItemType : InterfaceType<ITradableItem>
{
    protected override void Configure(IInterfaceTypeDescriptor<ITradableItem> descriptor)
    {
        descriptor.Field(t => t.TradableId)
            .Type<NonNullType<UuidType>>();

        descriptor.Field(t => t.RequiredBlockIndex)
            .Type<NonNullType<LongType>>();
    }
}
