using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class EquipmentInterfaceType : InterfaceType<Equipment>
{
    protected override void Configure(IInterfaceTypeDescriptor<Equipment> descriptor)
    {
        descriptor.Name("EquipmentInterface");
    }
}
