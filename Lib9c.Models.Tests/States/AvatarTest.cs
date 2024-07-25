using Bencodex.Types;
using Lib9c.Models.States;
using Lib9c.Models.Tests.Fixtures.States;
using Nekoyume.Model.State;

namespace Lib9c.Models.Tests.States;

public class AvatarTest
{
    [Fact]
    public void Test()
    {
        // Prepare target state
        var value = StateReader.ReadState("AvatarState");
        var target = new AvatarState((List)value);

        // serialize target state and deserialize as paired state
        var serialized = target.SerializeList();
        var paired = new Avatar(serialized);
        Assert.Equal(target.address, paired.Address);
        // ...

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = new AvatarState((List)bencoded);
        var serialized2 = target2.SerializeList();
        Assert.Equal(serialized, serialized2);
    }
}
