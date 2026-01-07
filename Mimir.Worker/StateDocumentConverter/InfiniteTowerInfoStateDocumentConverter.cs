using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class InfiniteTowerInfoStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        var state = new InfiniteTowerInfo(context.RawState);
        return new InfiniteTowerInfoDocument(
            context.BlockIndex,
            context.Address,
            state.Address,
            state.InfiniteTowerId,
            state
        );
    }
}





