using System.Text;
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
            .Description("Get the sheet by name.")
            .Argument("sheetName", a => a
                .Type<NonNullType<SheetNameType>>()
                .Description("The name of the sheet."))
            .Argument("encodeAsBase64", a => a
                .Type<BooleanType>()
                .Description("Indicates whether the sheet content should be encoded in Base64 format.")
                .DefaultValue(false))
            .Type<NonNullType<SheetType>>()
            .ResolveWith<SheetQueryTypeExtension>(_ => GetSheet(
                default!,
                default!,
                default!));
    }

    private static async Task<SheetObject> GetSheet(
        string sheetName,
        bool encodeAsBase64,
        [Service] TableSheetsRepository tableSheetsRepository)
    {
        var csv = await tableSheetsRepository.GetSheetAsync(
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
