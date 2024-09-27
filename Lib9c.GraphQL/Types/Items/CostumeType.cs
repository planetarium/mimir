using HotChocolate.Types;
using Lib9c.Models.Items;

namespace Lib9c.GraphQL.Types.Items;

public class CostumeType : ObjectType<Costume>
{
    protected override void Configure(IObjectTypeDescriptor<Costume> descriptor)
    {
        descriptor.Implements<ItemBaseInterfaceType>();
    }
}
