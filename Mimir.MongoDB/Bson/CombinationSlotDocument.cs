using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record CombinationSlotDocument(
    Nekoyume.Model.State.CombinationSlotState Object,
    Address AvatarAddress,
    int SlotIndex)
    : IMimirBsonDocument
{
    public Address Address => Object.address;
    public IValue Bencoded => Object.Serialize();
}
