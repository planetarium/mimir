using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class WorldInformationStateDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        return new WorldInformationDocument(
            context.BlockIndex,
            context.Address,
            new WorldInformationState(context.RawState)
        );
    }
}