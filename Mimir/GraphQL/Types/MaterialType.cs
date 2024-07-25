using Lib9c.GraphQL.Types;
using Lib9c.Models.Item;

namespace Mimir.GraphQL.Types;

public class MaterialType : ObjectType<TradableMaterial>
{
    protected override void Configure(IObjectTypeDescriptor<TradableMaterial> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor
            .Field(f => f.TradableId)
            .Description("The tradable ID of the item.")
            .Type<NonNullType<GuidType>>();
        descriptor
            .Field(f => f.ItemId)
            .Name("materialItemId")
            .Description("The ItemSheet ID of the item.")
            .Type<NonNullType<HashDigestSHA256Type>>();
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
        descriptor
            .Field(f => f.RequiredBlockIndex)
            .Description("The required block index of the item.")
            .Type<IntType>();
    }
}
