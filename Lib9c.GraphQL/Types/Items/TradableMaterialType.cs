using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class TradableMaterialType : ObjectType<TradableMaterial>
{
    protected override void Configure(IObjectTypeDescriptor<TradableMaterial> descriptor)
    {
        descriptor.Implements<MaterialInterfaceType>();
    }
}
