using Bencodex.Types;
using RuneState = Lib9c.Models.States.RuneState;

namespace Lib9c.Models.Tests.States;

public class RuneStateTest
{
    [Fact]
    public void Test()
    {
        var target = new Nekoyume.Model.State.RuneState(0, 0);
        var serialized = target.Serialize();
        var paired = new RuneState(serialized);
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);
        var target2 = new Nekoyume.Model.State.RuneState((List)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
