using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class DailyRewardState(Address address, long value) : State(address)
{
    public long Object { get; set; } = value;
}
