using Lib9c.GraphQL.Types;
using Mimir.GraphQL.Queries;
using Mimir.Repositories;

namespace Mimir.GraphQL.TypeExtensions;

public class SheetNamesQueryTypeExtension : ObjectTypeExtension<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field("sheetNames")
            .Argument("planetName", a => a.Type<NonNullType<PlanetNameEnumType>>())
            .Type<NonNullType<ListType<NonNullType<StringType>>>>()
            .ResolveWith<SheetNamesQueryTypeExtension>(t => GetSheetNames(default!, default!));
    }

    private static string[] GetSheetNames(
        string planetName,
        [Service] TableSheetsRepository tableSheetsRepository) =>
        tableSheetsRepository.GetSheetNames(planetName);
}
