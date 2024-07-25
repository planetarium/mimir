using Bencodex.Types;
using Lib9c.Models.Items;
using Nekoyume.TableData;

namespace Lib9c.Models.Tests.Items;

public class CostumeTest
{
    [Fact]
    public void Test()
    {
        // Prepare target state
        var row = new CostumeItemSheet.Row();
        row.Set(["0", "FullCostume", "1", "Normal", string.Empty]);
        var target = new Nekoyume.Model.Item.Costume(
            row,
            Guid.NewGuid());

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new Costume(serialized);
        // ...

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = new Nekoyume.Model.Item.Costume((Dictionary)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
