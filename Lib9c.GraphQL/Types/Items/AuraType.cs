using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class AuraType : ObjectType<Aura>
{
    protected override void Configure(IObjectTypeDescriptor<Aura> descriptor)
    {
        descriptor.Implements<EquipmentInterfaceType>();
    }
}
