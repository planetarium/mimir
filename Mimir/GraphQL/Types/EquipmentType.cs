using Lib9c.GraphQL.Types;
using Mimir.Models.Item;

namespace Mimir.GraphQL.Types;

public class EquipmentBaseType<T> : ObjectType<T>
    where T : Equipment
{
    protected override void Configure(IObjectTypeDescriptor<T> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor
            .Field(f => f.ItemId)
            .Name("equipmentItemId")
            .Description("The ItemSheet ID of the item.")
            .Type<NonNullType<GuidType>>();
        descriptor
            .Field(f => f.Grade)
            .Description("The grade of the item.")
            .Type<NonNullType<IntType>>();
        descriptor
            .Field(f => f.ItemType)
            .Description("The ItemType of the item.")
            .Type<NonNullType<EnumType<Nekoyume.Model.Item.ItemType>>>();
        descriptor
            .Field(f => f.ItemSubType)
            .Description("The ItemSubType of the item.")
            .Type<NonNullType<EnumType<Nekoyume.Model.Item.ItemSubType>>>();
        descriptor
            .Field(f => f.ElementalType)
            .Description("The ElementalType of the item.")
            .Type<NonNullType<EnumType<Nekoyume.Model.Elemental.ElementalType>>>();
        descriptor.Field(f => f.Level).Description("The level of the item.").Type<IntType>();
        descriptor.Field(f => f.Exp).Description("The exp of the item.").Type<LongType>();
        descriptor
            .Field(f => f.RequiredBlockIndex)
            .Description("The required block index of the item.")
            .Type<IntType>();
        descriptor
            .Field(f => f.Equipped)
            .Description("The equipped status of the item.")
            .Type<BooleanType>();
        descriptor
            .Field(f => f.Stat)
            .Description("The main stat type of the item.")
            .Type<DecimalStatType>();
    }
}
