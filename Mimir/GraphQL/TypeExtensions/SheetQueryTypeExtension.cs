using Lib9c.GraphQL.Objects;
using Lib9c.GraphQL.Types;
using Mimir.Enums;
using Mimir.GraphQL.Queries;
using Mimir.Repositories;

namespace Mimir.GraphQL.TypeExtensions;

public class SheetQueryTypeExtension : ObjectTypeExtension<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field("sheet")
            .Argument("planetName", a => a.Type<NonNullType<PlanetNameType>>())
            .Argument("sheetName", a => a.Type<NonNullType<SheetNameType>>())
            .Type<NonNullType<SheetType>>()
            .ResolveWith<SheetQueryTypeExtension>(t => GetSheet(default!, default!, default!));
    }

    private static async Task<SheetObject> GetSheet(
        string planetName,
        string sheetName,
        [Service] TableSheetsRepository tableSheetsRepository)
    {
        var csv = await tableSheetsRepository.GetSheet(planetName, sheetName, SheetFormat.Csv);
        return new SheetObject
        {
            Name = sheetName,
            Csv = csv,
        };
    }
}
