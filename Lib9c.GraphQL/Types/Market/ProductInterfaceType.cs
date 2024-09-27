using HotChocolate.Types;
using Lib9c.Models.Market;

namespace Lib9c.GraphQL.Types.Market;

public class ProductInterfaceType : InterfaceType<Product>
{
    protected override void Configure(IInterfaceTypeDescriptor<Product> descriptor)
    {
        descriptor.Name("ProductInterface");
    }
}
