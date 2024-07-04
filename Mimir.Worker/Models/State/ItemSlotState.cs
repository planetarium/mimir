using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class ItemSlotState(Address address, Nekoyume.Model.State.ItemSlotState itemSlotState)
    : State(address)
{
    public Nekoyume.Model.State.ItemSlotState Object { get; } = itemSlotState;

    public override IValue Serialize()
    {
        return Object.Serialize();
    }
}
