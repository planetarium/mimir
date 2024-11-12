using Lib9c.Models.Arena;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class ArenaParticipantDocumentConverter : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        var arenaParticipant = new ArenaParticipant(context.RawState);
        return new ArenaParticipantDocument(context.BlockIndex, context.Address, arenaParticipant);
    }
}
