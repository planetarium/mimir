using Bencodex.Types;
using Lib9c.Models.Items;
using Nekoyume.TableData;

namespace Lib9c.Models.Tests.Items;

public class CostumeTest
{
    [Fact]
    public void Test()
    {
        // Prepare legacy state
        var row = new CostumeItemSheet.Row();
        row.Set(new[] { "0", "FullCostume", "1", "Normal", string.Empty });
        var target = new Nekoyume.Model.Item.Costume(
            row,
            Guid.NewGuid());

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var deserialized = new Costume(serialized);

        // Check Deserialize from List
        Assert.Equal(target.Id, deserialized.Id);
        Assert.Equal(target.ItemType, deserialized.ItemType);
        Assert.Equal(target.ItemSubType, deserialized.ItemSubType);
        Assert.Equal(target.Grade, deserialized.Grade);
        Assert.Equal(target.ElementalType, deserialized.ElementalType);
        Assert.Equal(target.Equipped, deserialized.Equipped);
        Assert.Equal(target.SpineResourcePath, deserialized.SpineResourcePath);
        Assert.Equal(target.ItemId, deserialized.ItemId);
        Assert.Equal(target.RequiredBlockIndex, deserialized.RequiredBlockIndex);
    }
}
