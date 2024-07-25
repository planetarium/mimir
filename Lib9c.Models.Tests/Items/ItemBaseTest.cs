using Bencodex.Types;
using Lib9c.Models.Item;
using Lib9c.Models.Tests.Fixtures.Types.Items;
using Nekoyume.TableData;

namespace Lib9c.Models.Tests.Items;

public class ItemBaseTest
{
    [Fact]
    public void Test()
    {
        // Prepare target state
        var row = new ItemSheet.Row();
        row.Set(["0", "Weapon", "1", "Normal"]);
        var target = new VanillaItemBase(row);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new ItemBase(serialized);
        // ...

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = new VanillaItemBase((Dictionary)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
