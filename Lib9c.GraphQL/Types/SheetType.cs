using HotChocolate.Types;
using Lib9c.GraphQL.Objects;

namespace Lib9c.GraphQL.Types
{
    public class SheetType : ObjectType<SheetObject>
    {
        protected override void Configure(IObjectTypeDescriptor<SheetObject> descriptor)
        {
            descriptor.Field(f => f.Name)
                .Name("name")
                .Description("The name of the sheet.")
                .Type<NonNullType<SheetNameType>>();
            descriptor.Field(f => f.Csv)
                .Name("csv")
                .Description("The CSV content of the sheet.")
                .Type<StringType>();
            descriptor.Field(f => f.Json)
                .Name("json")
                .Description("The JSON content of the sheet.")
                .Type<StringType>();
        }
    }
}
