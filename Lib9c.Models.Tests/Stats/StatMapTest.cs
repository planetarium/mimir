using Bencodex.Types;
using Lib9c.Models.Stat;

namespace Lib9c.Models.Tests.Stats;

public class StatMapTest
{
    [Fact]
    public void Test()
    {
        // Prepare target state
        var target = new Nekoyume.Model.Stat.StatMap();

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new StatMap(serialized);
        // ...

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = new Nekoyume.Model.Stat.StatMap();
        target2.Deserialize((Dictionary)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
