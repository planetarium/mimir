using Bencodex.Types;
using Lib9c.Models.Items;
using Nekoyume.TableData;

namespace Lib9c.Models.Tests.Items;

public class MaterialTest
{
    [Fact]
    public void Test()
    {
        // Prepare legacy state
        var row = new MaterialItemSheet.Row();
        row.Set(new[] { "0", "EquipmentMaterial", "1", "Normal" });
        var target = new Nekoyume.Model.Item.Material(row);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var deserialized = new Material(serialized);

        // Check Deserialize from List
        Assert.Equal(0, deserialized.Id);
        Assert.Equal(Nekoyume.Model.Item.ItemType.Material, deserialized.ItemType);
        Assert.Equal(Nekoyume.Model.Item.ItemSubType.EquipmentMaterial, deserialized.ItemSubType);
        Assert.Equal(1, deserialized.Grade);
        Assert.Equal(Nekoyume.Model.Elemental.ElementalType.Normal, deserialized.ElementalType);
        Assert.Equal(target.ItemId, deserialized.ItemId);
    }
}
