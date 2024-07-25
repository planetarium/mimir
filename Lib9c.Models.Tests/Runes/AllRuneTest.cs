using Bencodex.Types;
using Lib9c.Models.Runes;
using Nekoyume.Model.State;

namespace Lib9c.Models.Tests.Runes;

public class AllRuneTest
{
    [Fact]
    public void Test()
    {
        var target = new AllRuneState(0, 0);
        var serialized = target.Serialize();
        var paired = new AllRune(serialized);
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);
        var target2 = new AllRuneState((List)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
