using HotChocolate.Types;
using Mimir.Models.Product;

namespace Mimir.GraphQL.Types;

public class StatType : ObjectType<Stat>
{
    protected override void Configure(IObjectTypeDescriptor<Stat> descriptor)
    {
        descriptor.Field(s => s.StatType).Type<IntType>();
        descriptor.Field(s => s.BaseValue).Type<IntType>();
        descriptor.Field(s => s.AdditionalValue).Type<IntType>();
        descriptor.Field(s => s.TotalValue).Type<IntType>();
    }
}
