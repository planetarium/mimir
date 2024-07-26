// using Mimir.GraphQL.Queries;
// using Mimir.Repositories;
//
// namespace Mimir.GraphQL.TypeExtensions;
//
// public class SheetNamesQueryTypeExtension : ObjectTypeExtension<Query>
// {
//     protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
//     {
//         descriptor
//             .Field("sheetNames")
//             .Type<NonNullType<ListType<NonNullType<StringType>>>>()
//             .ResolveWith<SheetNamesQueryTypeExtension>(t => GetSheetNames(default!));
//     }
//
//     private static string[] GetSheetNames(
//         [Service] TableSheetsRepository tableSheetsRepository) =>
//         tableSheetsRepository.GetSheetNames();
// }
