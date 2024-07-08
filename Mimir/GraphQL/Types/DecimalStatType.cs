using Mimir.Models.Stat;

namespace Lib9c.GraphQL.Types;

public class DecimalStatType : ObjectType<DecimalStat>
{
    protected override void Configure(IObjectTypeDescriptor<DecimalStat> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(f => f.StatType).Type<NonNullType<EnumType<Nekoyume.Model.Stat.StatType>>>();

        descriptor.Field(f => f.BaseValue).Type<NonNullType<DecimalType>>();

        descriptor.Field(f => f.AdditionalValue).Type<NonNullType<DecimalType>>();
    }
}
