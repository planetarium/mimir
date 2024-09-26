using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class WeaponType : ObjectType<Weapon>
{
    protected override void Configure(IObjectTypeDescriptor<Weapon> descriptor)
    {
        descriptor.Implements<EquipmentInterfaceType>();
    }
}
