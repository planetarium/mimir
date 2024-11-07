using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class AvatarStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        return new AvatarDocument(
            context.BlockIndex,
            context.Address,
            new AvatarState(context.RawState)
        );
    }
}
