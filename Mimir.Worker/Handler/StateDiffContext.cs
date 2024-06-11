using Bencodex.Types;
using HeadlessGQL;
using Libplanet.Crypto;

namespace Mimir.Worker.Handler;

public class StateDiffContext
{
    public Address Address { get; set; }
    public IValue RawState { get; set; }
}
