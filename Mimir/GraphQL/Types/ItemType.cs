// using Lib9c.GraphQL.Types;
// using Mimir.GraphQL.Objects;
// using Nekoyume.Model.Elemental;
// using Nekoyume.Model.Item;
// using Nekoyume.Model.Stat;
//
// namespace Mimir.GraphQL.Types;
//
// public class ItemType : ObjectType<ItemObject>
// {
//     protected override void Configure(IObjectTypeDescriptor<ItemObject> descriptor)
//     {
//         descriptor
//             .Field(f => f.ItemSheetId)
//             .Description("The ItemSheet ID of the item.")
//             .Type<NonNullType<IntType>>();
//         descriptor
//             .Field(f => f.Grade)
//             .Description("The grade of the item.")
//             .Type<NonNullType<IntType>>();
//         descriptor
//             .Field(f => f.ItemType)
//             .Description("The ItemType of the item.")
//             .Type<NonNullType<EnumType<Nekoyume.Model.Item.ItemType>>>();
//         descriptor
//             .Field(f => f.ItemSubType)
//             .Description("The ItemSubType of the item.")
//             .Type<NonNullType<EnumType<ItemSubType>>>();
//         descriptor
//             .Field(f => f.ElementalType)
//             .Description("The ElementalType of the item.")
//             .Type<NonNullType<EnumType<ElementalType>>>();
//         descriptor
//             .Field(f => f.Count)
//             .Description("The count of the item.")
//             .Type<NonNullType<IntType>>();
//         descriptor
//             .Field(f => f.Locked)
//             .Description("The locked status of the item.")
//             .Type<NonNullType<BooleanType>>();
//         descriptor
//             .Field(f => f.Level)
//             .Description("The level of the item.")
//             .Type<IntType>();
//         descriptor
//             .Field(f => f.Exp)
//             .Description("The exp of the item.")
//             .Type<LongType>();
//         descriptor
//             .Field(f => f.RequiredBlockIndex)
//             .Description("The required block index of the item.")
//             .Type<IntType>();
//         descriptor
//             .Field(f => f.FungibleId)
//             .Description("The Fungible ID of the item.")
//             .Type<HashDigestSHA256Type>();
//         descriptor
//             .Field(f => f.NonFungibleId)
//             .Description("The Non-Fungible ID of the item.")
//             .Type<GuidType>();
//         descriptor
//             .Field(f => f.TradableId)
//             .Description("The Tradable ID of the item.")
//             .Type<GuidType>();
//         descriptor
//             .Field(f => f.Equipped)
//             .Description("The equipped status of the item.")
//             .Type<BooleanType>();
//         descriptor
//             .Field(f => f.MainStatType)
//             .Description("The main stat type of the item.")
//             .Type<EnumType<StatType>>();
//         descriptor
//             .Field(f => f.StatsMap)
//             .Description("The stats map of the item.")
//             .Type<StatMapType>();
//         descriptor
//             .Field(f => f.Skills)
//             .Description("The skills of the item.")
//             .Type<ListType<SkillType>>();
//         descriptor
//             .Field(f => f.BuffSkills)
//             .Description("The buff skills of the item.")
//             .Type<ListType<SkillType>>();
//     }
// }
