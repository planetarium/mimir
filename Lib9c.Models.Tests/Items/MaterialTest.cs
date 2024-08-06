using Bencodex.Types;
using Lib9c.Models.Items;
using Nekoyume.TableData;

namespace Lib9c.Models.Tests.Items;

public class MaterialTest
{
    [Fact]
    public void Test()
    {
        // Prepare target state
        var row = new MaterialItemSheet.Row();
        row.Set(new[] { "0", "EquipmentMaterial", "1", "Normal" });
        var target = new Nekoyume.Model.Item.Material(row);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new Material(serialized);
        // ...

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = new Nekoyume.Model.Item.Material((Dictionary)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
