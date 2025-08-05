using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Bencodex.Types;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class DailyRewardStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        if (context.RawState is not Integer value)
            throw new ArgumentException(
                $"Invalid state type. Expected {nameof(Integer)}, got {context.RawState.GetType().Name}.",
                nameof(context.RawState)
            );

        return new DailyRewardDocument(context.BlockIndex, context.Address, value);
    }
}
