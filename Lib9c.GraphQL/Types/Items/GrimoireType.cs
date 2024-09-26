using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class GrimoireType : ObjectType<Grimoire>
{
    protected override void Configure(IObjectTypeDescriptor<Grimoire> descriptor)
    {
        descriptor.Implements<EquipmentInterfaceType>();
    }
}
