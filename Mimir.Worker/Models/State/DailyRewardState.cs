using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class DailyRewardState(long value) : IBencodable
{
    public long Object { get; set; } = value;

    public IValue Bencoded => Object.Serialize();
}
