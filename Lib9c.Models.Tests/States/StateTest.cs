using Lib9c.Models.States;
using Lib9c.Models.Tests.Fixtures.Types.States;
using Libplanet.Crypto;

namespace Lib9c.Models.Tests.States;

public class StateTest
{
    [Fact]
    public void Test()
    {
        // Prepare target state
        var target = new VanillaState(new PrivateKey().Address);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new State(serialized);
        Assert.Equal(target.address, paired.Address);

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);
    }
}
