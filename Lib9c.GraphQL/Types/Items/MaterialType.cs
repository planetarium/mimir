using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class MaterialType : ObjectType<Material>
{
    protected override void Configure(IObjectTypeDescriptor<Material> descriptor)
    {
        descriptor.Implements<ItemBaseInterfaceType>();
    }
}
