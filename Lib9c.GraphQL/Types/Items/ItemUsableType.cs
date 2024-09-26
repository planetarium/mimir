using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class ItemUsableType : ObjectType<ItemUsable>
{
    protected override void Configure(IObjectTypeDescriptor<ItemUsable> descriptor)
    {
        descriptor.Implements<ItemBaseInterfaceType>();
    }
}
