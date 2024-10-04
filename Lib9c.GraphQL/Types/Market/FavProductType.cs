using HotChocolate.Types;
using Lib9c.Models.Market;

namespace Lib9c.GraphQL.Types.Market;

public class FavProductType : ObjectType<FavProduct>
{
    protected override void Configure(IObjectTypeDescriptor<FavProduct> descriptor)
    {
        descriptor.Implements<ProductInterfaceType>();
    }
}
