using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Lib9c.Models.Tests.Fixtures.Types;

public class VanillaState : State
{
    public VanillaState(Address address) : base(address)
    {
    }

    public VanillaState(IValue iValue) : base(iValue)
    {
    }
}
