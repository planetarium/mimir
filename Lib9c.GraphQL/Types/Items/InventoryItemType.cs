using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class InventoryItemType : ObjectType<InventoryItem>
{
    protected override void Configure(IObjectTypeDescriptor<InventoryItem> descriptor)
    {
        descriptor.Ignore(f => f.Lock);
    }
}
