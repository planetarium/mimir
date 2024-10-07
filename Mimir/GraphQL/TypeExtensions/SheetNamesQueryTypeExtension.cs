using Mimir.GraphQL.Queries;
using Mimir.MongoDB.Repositories;

namespace Mimir.GraphQL.TypeExtensions;

public class SheetNamesQueryTypeExtension : ObjectTypeExtension<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field("sheetNames")
            .Description("Get the names of all sheets.")
            .Type<NonNullType<ListType<NonNullType<StringType>>>>()
            .ResolveWith<SheetNamesQueryTypeExtension>(t => GetSheetNamesAsync(default!));
    }

    private static async Task<string[]> GetSheetNamesAsync(
        [Service] TableSheetsRepository tableSheetsRepository) =>
        await tableSheetsRepository.GetSheetNamesAsync();
}
