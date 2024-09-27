using HotChocolate.Types;

namespace Lib9c.GraphQL.Types.Sheets;

public class SheetNameType : StringType
{
    public SheetNameType() : base(nameof(SheetNameType))
    {
    }
}
