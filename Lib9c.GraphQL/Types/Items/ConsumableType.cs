using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class ConsumableType : ObjectType<Consumable>
{
    protected override void Configure(IObjectTypeDescriptor<Consumable> descriptor)
    {
        descriptor.Implements<ItemUsableInterfaceType>();
    }
}
