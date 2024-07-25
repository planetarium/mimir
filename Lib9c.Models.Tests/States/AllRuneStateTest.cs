using Bencodex.Types;
using AllRuneState = Lib9c.Models.States.AllRuneState;

namespace Lib9c.Models.Tests.States;

public class AllRuneStateTest
{
    [Fact]
    public void Test()
    {
        var target = new Nekoyume.Model.State.AllRuneState(0, 0);
        var serialized = target.Serialize();
        var paired = new AllRuneState(serialized);
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);
        var target2 = new Nekoyume.Model.State.AllRuneState((List)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
