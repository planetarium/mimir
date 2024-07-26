using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public class CombinationSlotState(
    Address avatarAddress,
    int slotIndex,
    Nekoyume.Model.State.CombinationSlotState combinationSlotState
) : IBencodable
{
    public int slotIndex { get; set; } = slotIndex;

    public Address avatarAddress { get; set; } = avatarAddress;

    public Nekoyume.Model.State.CombinationSlotState Object { get; set; } = combinationSlotState;

    public IValue Bencoded => Object.Serialize();
}
