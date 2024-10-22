using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class AllCombinationSlotStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        return new AllCombinationSlotStateDocument(context.Address, new AllCombinationSlotState(context.RawState));
    }
}