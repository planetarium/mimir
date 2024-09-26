using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class NecklaceType : ObjectType<Necklace>
{
    protected override void Configure(IObjectTypeDescriptor<Necklace> descriptor)
    {
        descriptor.Implements<EquipmentInterfaceType>();
    }
}
