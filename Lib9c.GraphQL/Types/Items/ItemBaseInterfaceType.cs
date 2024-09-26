using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class ItemBaseInterfaceType : InterfaceType<ItemBase>
{
    protected override void Configure(IInterfaceTypeDescriptor<ItemBase> descriptor)
    {
        descriptor.Name("ItemBaseInterface");
    }
}
