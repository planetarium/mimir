using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record CombinationSlotDocument(
    Address AvatarAddress,
    int SlotIndex,
    Nekoyume.Model.State.CombinationSlotState Object)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
