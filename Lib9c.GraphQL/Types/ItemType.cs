using HotChocolate.Types;
using Nekoyume.Model.Item;

namespace Lib9c.GraphQL.Types;

public class ItemType : InterfaceType<IItem>
{
    protected override void Configure(IInterfaceTypeDescriptor<IItem> descriptor)
    {
        descriptor
            .Field(f => f.ItemType)
            .Description("The ItemType of the item.")
            .Type<NonNullType<ItemTypeEnumType>>();
        descriptor
            .Field(f => f.ItemSubType)
            .Description("The ItemSubType of the item.")
            .Type<NonNullType<ItemSubTypeEnumType>>();
    }
}
