// using Mimir.GraphQL.Objects;
//
// namespace Mimir.GraphQL.Types;
//
// public class RuneType : ObjectType<RuneObject>
// {
//     protected override void Configure(IObjectTypeDescriptor<RuneObject> descriptor)
//     {
//         descriptor
//             .Field(f => f.RuneSheetId)
//             .Description("The RuneSheet ID of the rune.")
//             .Type<NonNullType<IntType>>();
//         descriptor
//             .Field(f => f.Level)
//             .Description("The level of the rune.")
//             .Type<NonNullType<IntType>>();
//     }
// }
