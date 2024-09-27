using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class RingType : ObjectType<Ring>
{
    protected override void Configure(IObjectTypeDescriptor<Ring> descriptor)
    {
        descriptor.Implements<EquipmentInterfaceType>();
    }
}
