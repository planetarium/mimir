using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.Stake;

namespace Mimir.Worker.Models;

public class StakeState(StakeStateV2 stakeState) : IBencodable
{
    public StakeStateV2 Object { get; set; } = stakeState;
    public IValue Bencoded => Object.Serialize();
}
