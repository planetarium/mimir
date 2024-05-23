using HotChocolate.Types;

namespace Lib9c.GraphQL.Types;

public class PlanetNameType : StringType
{
    public PlanetNameType() : base(nameof(PlanetNameType))
    {
    }
}
