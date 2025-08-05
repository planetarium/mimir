using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Bencodex.Types;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class ActionPointStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        if (context.RawState is not Integer value)
            throw new InvalidCastException(
                $"{nameof(context.RawState)} Invalid state type. Expected {nameof(Integer)}, got {context.RawState.GetType().Name}."
            );

        return new ActionPointDocument(context.BlockIndex, context.Address, value);
    }
}
