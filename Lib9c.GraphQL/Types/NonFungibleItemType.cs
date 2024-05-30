using HotChocolate.Types;
using Nekoyume.Model.Item;

namespace Lib9c.GraphQL.Types;

public class NonFungibleItemType : InterfaceType<INonFungibleItem>
{
    protected override void Configure(IInterfaceTypeDescriptor<INonFungibleItem> descriptor)
    {
        descriptor
            .Field(f => f.NonFungibleId)
            .Description("The non-fungible ID of the item.")
            .Type<NonNullType<GuidType>>();
    }
}
