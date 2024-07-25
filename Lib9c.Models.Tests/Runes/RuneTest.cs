using Bencodex.Types;
using Lib9c.Models.Runes;
using Nekoyume.Model.State;

namespace Lib9c.Models.Tests.Runes;

public class RuneTest
{
    [Fact]
    public void Test()
    {
        var target = new RuneState(0, 0);
        var serialized = target.Serialize();
        var paired = new Rune(serialized);
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);
        var target2 = new RuneState((List)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
