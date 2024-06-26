using Libplanet.Crypto;
using Mimir.Models;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class RuneSlotState(Address address, RuneSlots runeSlots) : State(address)
{
    public RuneSlots Object { get; } = runeSlots;
}
