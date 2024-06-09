using Libplanet.Crypto;
using Nekoyume.Model.Arena;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class ArenaScoreState : State
{
    public ArenaScore Object;

    public ArenaScoreState(Address address, ArenaScore arenaScore)
        : base(address)
    {
        Object = arenaScore;
    }
}
