using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class AllCombinationSlotStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        return new AllCombinationSlotStateDocument(
            context.BlockIndex,
            context.Address,
            new AllCombinationSlotState(context.RawState)
        );
    }
}
