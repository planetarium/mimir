using Bencodex.Types;
using Mimir.MongoDB.Bson;
using Nekoyume.Model;

namespace Mimir.Worker.Handler;

public class WorldInformationStateHandler : IStateHandler
{
    public MimirBsonDocument ConvertToState(StateDiffContext context)
    {
        if (context.RawState is Dictionary dict)
        {
            return new WorldInformationDocument(context.Address, new WorldInformation(dict));
        }

        throw new InvalidCastException(
            $"{nameof(context.RawState)} Invalid state type. Expected Dictionary."
        );
    }
}
