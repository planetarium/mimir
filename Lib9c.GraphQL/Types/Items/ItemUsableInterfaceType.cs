using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class ItemUsableInterfaceType : InterfaceType<ItemUsable>
{
    protected override void Configure(IInterfaceTypeDescriptor<ItemUsable> descriptor)
    {
        descriptor.Name("ItemUsableInterface");
    }
}
