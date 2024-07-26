// using Lib9c.GraphQL.Types;
// using Lib9c.Models.Items;
//
// namespace Mimir.GraphQL.Types;
//
// public class ConsumableType : ObjectType<Consumable>
// {
//     protected override void Configure(IObjectTypeDescriptor<Consumable> descriptor)
//     {
//         descriptor.BindFieldsExplicitly();
//         descriptor
//             .Field(f => f.ItemId)
//             .Description("The ItemSheet ID of the item.")
//             .Type<NonNullType<GuidType>>();
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
//             .Type<NonNullType<EnumType<Nekoyume.Model.Item.ItemSubType>>>();
//         descriptor
//             .Field(f => f.ElementalType)
//             .Description("The ElementalType of the item.")
//             .Type<NonNullType<EnumType<Nekoyume.Model.Elemental.ElementalType>>>();
//         descriptor
//             .Field(f => f.RequiredBlockIndex)
//             .Description("The required block index of the item.")
//             .Type<IntType>();
//         descriptor
//             .Field(f => f.Stats)
//             .Description("The stats of the item.")
//             .Type<ListType<DecimalStatType>>();
//     }
// }
