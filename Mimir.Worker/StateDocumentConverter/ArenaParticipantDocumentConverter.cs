using Lib9c.Models.Arena;
using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class ArenaParticipantDocumentConverter : IStateDocumentConverter
{
    [Obsolete("Use ConvertToDocument(AddressStatePair, int, int, SimplifiedAvatarState) instead.")]
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        throw new InvalidOperationException(
            "ConvertToDocument method is not implemented for the given parameters.");
    }

    public static MimirBsonDocument ConvertToDocument(
        AddressStatePair context,
        int championshipId,
        int round,
        SimplifiedAvatarState simplifiedAvatar)
    {
        var arenaParticipant = new ArenaParticipant(context.RawState);
        return new ArenaParticipantDocument(
            context.BlockIndex,
            context.Address,
            arenaParticipant,
            championshipId,
            round,
            simplifiedAvatar);
    }
}
