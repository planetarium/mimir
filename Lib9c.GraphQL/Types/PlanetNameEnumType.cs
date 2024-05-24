using HotChocolate.Types;

namespace Lib9c.GraphQL.Types;

public class PlanetNameEnumType : EnumType<string>
{
    protected override void Configure(IEnumTypeDescriptor<string> descriptor)
    {
        descriptor
            .Name("PlanetName")
            .Description("The name of the planet.\n" +
                         "See https://planets.nine-chronicles.com/planets/ or" +
                         " https://planets-internal.nine-chronicles.com/planets/ for more information.")
            .BindValuesExplicitly();
        descriptor
            .Value("odin")
            .Name("ODIN")
            .Description("The name of the planet odin.");
        descriptor
            .Value("heimdall")
            .Name("HEIMDALL")
            .Description("The name of the planet heimdall.");
    }
}
