using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class BeltType : ObjectType<Belt>
{
    protected override void Configure(IObjectTypeDescriptor<Belt> descriptor)
    {
        descriptor.Implements<EquipmentInterfaceType>();
    }
}
