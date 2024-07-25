using Bencodex.Types;
using Lib9c.Models.Items;
using Nekoyume.TableData;

namespace Lib9c.Models.Tests.Items;

public class ConsumableTest
{
    [Fact]
    public void Test()
    {
        // Prepare target state
        var row = new ConsumableItemSheet.Row();
        row.Set(["0", "Food", "1", "Normal", string.Empty, "HP", "0", "ATK", "0"]);
        var target = new Nekoyume.Model.Item.Consumable(
            row,
            Guid.NewGuid(),
            requiredBlockIndex: 0);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new Consumable(serialized);
        // ...

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = new Nekoyume.Model.Item.Consumable((Dictionary)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
