using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class RuneSlotState(Address address, Nekoyume.Model.State.RuneSlotState runeSlotState) : State(address)
{
    public Nekoyume.Model.State.RuneSlotState Object { get; } = runeSlotState;
}
