using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.State;

namespace Mimir.Worker.StateDocumentConverter;

public class RaidCpStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        if (context.RawState is not List list)
            throw new InvalidCastException(
                $"{nameof(context.RawState)} Invalid state type. Expected {nameof(List)}, got {context.RawState.GetType().Name}."
            );

        var cp = list[0].ToInteger();

        return new RaidCpDocument(context.BlockIndex, context.Address, cp);
    }
}
