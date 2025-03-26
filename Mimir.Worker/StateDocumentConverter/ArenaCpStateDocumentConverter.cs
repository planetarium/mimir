using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Nekoyume.Model.EnumType;

namespace Mimir.Worker.StateDocumentConverter;

public class ArenaCpStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        if (context.RawState is not List list)
            throw new InvalidCastException(
                $"{nameof(context.RawState)} Invalid state type. Expected {nameof(List)}, got {context.RawState.GetType().Name}."
            );

        var cp = (int)((Integer)list[0]).Value;
        
        return new ArenaCpDocument(
            context.BlockIndex, 
            context.Address,
            cp
        );
    }
} 