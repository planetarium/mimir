using Libplanet.Crypto;
using Nekoyume.Model.Stake;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class StakeState(Address address, StakeStateV2 stakeState) : State(address)
{
    public StakeStateV2 Object { get; set; } = stakeState;
}
