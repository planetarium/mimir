using Libplanet.Crypto;
using Nekoyume.Model.Arena;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class ArenaInformationState : State
{
    public ArenaInformation Object;

    public ArenaInformationState(Address address, ArenaInformation arenaInformation)
        : base(address)
    {
        Object = arenaInformation;
    }
}
