using Bencodex.Types;
using Libplanet.Crypto;

namespace Lib9c.Models.Tests.Fixtures.Types;

public class VanillaState : Nekoyume.Model.State.State
{
    public VanillaState(Address address) : base(address)
    {
    }

    public VanillaState(IValue iValue) : base(iValue)
    {
    }
}
