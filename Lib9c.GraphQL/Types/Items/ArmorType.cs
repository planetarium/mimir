using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class ArmorType : ObjectType<Armor>
{
    protected override void Configure(IObjectTypeDescriptor<Armor> descriptor)
    {
        descriptor.Implements<EquipmentInterfaceType>();
    }
}
