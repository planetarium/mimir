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
        descriptor.Field(s => s.HasTotalValueAsLong).Type<BooleanType>();
        descriptor.Field(s => s.HasBaseValueAsLong).Type<BooleanType>();
        descriptor.Field(s => s.HasAdditionalValueAsLong).Type<BooleanType>();
        descriptor.Field(s => s.HasBaseValue).Type<BooleanType>();
        descriptor.Field(s => s.HasAdditionalValue).Type<BooleanType>();
        descriptor.Field(s => s.BaseValueAsLong).Type<LongType>();
        descriptor.Field(s => s.AdditionalValueAsLong).Type<LongType>();
        descriptor.Field(s => s.TotalValueAsLong).Type<LongType>();
        descriptor.Field(s => s.TotalValue).Type<IntType>();
    }
}
