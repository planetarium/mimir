using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class MaterialInterfaceType : InterfaceType<Material>
{
    protected override void Configure(IInterfaceTypeDescriptor<Material> descriptor)
    {
        descriptor.Name("MaterialInterface");
    }
}
