using Libplanet.Crypto;
using Nekoyume.Model.Arena;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class ArenaScoreState : State
{
    public ArenaScore ArenaScore;

    public ArenaScoreState(Address address, ArenaScore arenaScore)
        : base(address)
    {
        ArenaScore = arenaScore;
    }
}
