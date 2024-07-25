using Bencodex.Types;
using Lib9c.Models.Items;
using Lib9c.Models.Tests.Fixtures.Types.Items;
using Nekoyume.TableData;

namespace Lib9c.Models.Tests.Items;

public class ItemUsableTest
{
    [Fact]
    public void Test()
    {
        // Prepare target state
        var row = new EquipmentItemSheet.Row();
        row.Set(["0", "Weapon", "1", "Normal", string.Empty, "HP", "0", "0", string.Empty, string.Empty]);
        var target = new VanillaItemUsable(
            row,
            Guid.NewGuid(),
            requiredBlockIndex: 0);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new ItemUsable(serialized);
        // ...

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = new VanillaItemUsable((Dictionary)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
