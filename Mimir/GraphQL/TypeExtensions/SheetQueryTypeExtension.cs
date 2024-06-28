using System.Text;
using Lib9c.GraphQL.Enums;
using Lib9c.GraphQL.Types;
using Mimir.Enums;
using Mimir.GraphQL.Objects;
using Mimir.GraphQL.Queries;
using Mimir.GraphQL.Types;
using Mimir.Repositories;

namespace Mimir.GraphQL.TypeExtensions;

public class SheetQueryTypeExtension : ObjectTypeExtension<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field("sheet")
            .Argument("planetName", a => a.Type<NonNullType<EnumType<PlanetName>>>())
            .Argument("sheetName", a => a.Type<NonNullType<SheetNameType>>())
            .Argument("encodeAsBase64", a => a.Type<BooleanType>().DefaultValue(false))
            .Type<NonNullType<SheetType>>()
            .ResolveWith<SheetQueryTypeExtension>(_ => GetSheet(
                default!,
                default!,
                default!,
                default!));
    }

    private static async Task<SheetObject> GetSheet(
        string planetName,
        string sheetName,
        bool encodeAsBase64,
        [Service] TableSheetsRepository tableSheetsRepository)
    {
        var csv = await tableSheetsRepository.GetSheetAsync(
            planetName,
            sheetName,
            SheetFormat.Csv);
        if (encodeAsBase64)
        {
            csv = Convert.ToBase64String(Encoding.UTF8.GetBytes(csv));
        }

        return new SheetObject
        {
            Name = sheetName,
            Csv = csv,
        };
    }
}
