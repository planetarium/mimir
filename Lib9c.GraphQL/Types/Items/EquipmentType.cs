using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class EquipmentType : ObjectType<Equipment>
{
    protected override void Configure(IObjectTypeDescriptor<Equipment> descriptor)
    {
        descriptor.Implements<ItemUsableInterfaceType>();
    }
}
