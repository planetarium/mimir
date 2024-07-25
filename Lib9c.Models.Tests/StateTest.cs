using Lib9c.Models.Tests.Fixtures.Types;
using Libplanet.Crypto;
using Lib9c.Models;

namespace Lib9c.Models.Tests;

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

        // serialize target state and deserialize as paired state
        serialized = target.SerializeV2();
        paired = new State(serialized);
        Assert.Equal(target.address, paired.Address);

        // serialize target state and deserialize as paired state
        serialized = target.SerializeList();
        paired = new State(serialized);
        Assert.Equal(target.address, paired.Address);

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = new VanillaState(bencoded);
        var serialized2 = target2.SerializeList();
        Assert.Equal(serialized, serialized2);
    }
}
