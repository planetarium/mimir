using HotChocolate.Types;
using Nekoyume.Model.Item;

namespace Lib9c.GraphQL.Types.Items;

public class FungibleItemType : InterfaceType<IFungibleItem>
{
    protected override void Configure(IInterfaceTypeDescriptor<IFungibleItem> descriptor)
    {
        descriptor
            .Field(t => t.FungibleId)
            .Description("The fungible ID of the item.")
            .Type<NonNullType<HashDigestSHA256Type>>();
    }
}
