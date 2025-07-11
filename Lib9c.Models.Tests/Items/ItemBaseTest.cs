using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using Bencodex.Types;
using Lib9c.Models.Tests.Fixtures.Types.Items;
using Libplanet.Common;
using Nekoyume.Model.Elemental;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using ItemBase = Lib9c.Models.Items.ItemBase;

namespace Lib9c.Models.Tests.Items;

public class ItemBaseTest
{
    [Fact]
    public void Test()
    {
        // Prepare legacy state
        var dictSerialized = Dictionary.Empty
            .Add("id", 0.Serialize())
            .Add("item_type", ItemType.Material.Serialize())
            .Add("item_sub_type", ItemSubType.FoodMaterial.Serialize())
            .Add("grade", 1.Serialize())
            .Add("elemental_type", ElementalType.Normal.Serialize());

        var target = ItemFactory.Deserialize(dictSerialized);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var deserialized = new ItemBase(serialized);

        // Check Deserialize from List
        Assert.Equal(0, deserialized.Id);
        Assert.Equal(ItemType.Material, deserialized.ItemType);
        Assert.Equal(ItemSubType.FoodMaterial, deserialized.ItemSubType);
        Assert.Equal(1, deserialized.Grade);
        Assert.Equal(ElementalType.Normal, deserialized.ElementalType);
    }
}
