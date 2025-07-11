using Bencodex.Types;
using Nekoyume.Model.Elemental;
using Nekoyume.Model.Item;
using Nekoyume.Model.Stat;
using Nekoyume.Model.State;

namespace Lib9c.Models.Tests.Items;

public class ConsumableTest
{
    [Fact]
    public void Test()
    {
        // Prepare legacy state
        var legacyStatsMapDict = List.Empty
            .Add(
                Dictionary.Empty
                    .Add("statType", StatType.HP.Serialize())
                    .Add("value", 100m.Serialize())
            )
            .Add(
                Dictionary.Empty
                    .Add("statType", StatType.ATK.Serialize())
                    .Add("value", 50m.Serialize())
            );

        var legacyDict = Dictionary.Empty
            .Add((Text)"id", 0.Serialize())
            .Add((Text)"grade", 1.Serialize())
            .Add((Text)"item_type", ItemType.Consumable.Serialize())
            .Add((Text)"item_sub_type", ItemSubType.Food.Serialize())
            .Add((Text)"elemental_type", ElementalType.Normal.Serialize())
            .Add((Text)"itemId", Guid.NewGuid().Serialize())
            .Add((Text)"stats", legacyStatsMapDict)
            .Add((Text)"skills", new List())
            .Add((Text)"buffSkills", new List())
            .Add((Text)"requiredBlockIndex", 1000L.Serialize());

        var target = (Consumable)ItemFactory.Deserialize(legacyDict);
        var hpStat = target.Stats.Single(i => i.StatType == StatType.HP);
        Assert.Equal(100m, hpStat.BaseValue);
        var atkStat = target.Stats.Single(i => i.StatType == StatType.ATK);
        Assert.Equal(50m, atkStat.BaseValue);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var deserialized = new Models.Items.Consumable(serialized);

        // Check Deserialize from List
        Assert.Equal(target.Id, deserialized.Id);
        Assert.Equal(target.ItemType, deserialized.ItemType);
        Assert.Equal(target.ItemSubType, deserialized.ItemSubType);
        Assert.Equal(target.Grade, deserialized.Grade);
        Assert.Equal(target.ElementalType, deserialized.ElementalType);
        Assert.Equal(target.ItemId, deserialized.ItemId);
        Assert.Equal(target.RequiredBlockIndex, deserialized.RequiredBlockIndex);
        Assert.Equal(target.Stats.Count, deserialized.Stats.Count);
    }
}
