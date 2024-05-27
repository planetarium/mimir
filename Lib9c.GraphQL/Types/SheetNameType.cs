using HotChocolate.Types;

namespace Lib9c.GraphQL.Types;

public class SheetNameType : StringType
{
    public SheetNameType() : base(nameof(SheetNameType))
    {
    }
}
