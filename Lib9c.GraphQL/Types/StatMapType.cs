using HotChocolate.Types;
using Nekoyume.Model.Stat;

namespace Lib9c.GraphQL.Types;

public class StatMapType : ObjectType<StatMap>
{
    protected override void Configure(IObjectTypeDescriptor<StatMap> descriptor)
    {
        descriptor.BindFields(BindingBehavior.Explicit);
        descriptor
            .Field(f => f.HP)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.ATK)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.DEF)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.CRI)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.HIT)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.SPD)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.DRV)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.DRR)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.CDMG)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.ArmorPenetration)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.Thorn)
            .Type<NonNullType<LongType>>();

        descriptor
            .Field(f => f.BaseHP)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.BaseATK)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.BaseDEF)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.BaseCRI)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.BaseHIT)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.BaseSPD)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.BaseDRV)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.BaseDRR)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.BaseCDMG)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.BaseArmorPenetration)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.BaseThorn)
            .Type<NonNullType<LongType>>();

        descriptor
            .Field(f => f.AdditionalHP)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.AdditionalATK)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.AdditionalDEF)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.AdditionalCRI)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.AdditionalHIT)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.AdditionalSPD)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.AdditionalDRV)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.AdditionalDRR)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.AdditionalCDMG)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.AdditionalArmorPenetration)
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.AdditionalThorn)
            .Type<NonNullType<LongType>>();
    }
}
