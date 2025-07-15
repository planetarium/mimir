using Bencodex.Types;
using Lib9c.Models.Extensions;
using Nekoyume.Model.Elemental;
using Nekoyume.Model.Item;
using Nekoyume.Model.Skill;
using Nekoyume.Model.Stat;

namespace Lib9c.Models.Tests.Items;

public class EquipmentTest
{
    [Fact]
    public void Test()
    {
        // Prepare legacy state
        var legacyStatsMapDict = (Dictionary) Dictionary.Empty
            .Add(
                StatType.HP.Serialize(),
                Dictionary.Empty
                    .Add("statType", StatType.HP.Serialize())
                    .Add("value", 2352.7369m.Serialize())
                    .Add("additionalValue", 0m.Serialize()))
            .Add(
                StatType.ATK.Serialize(),
                Dictionary.Empty
                    .Add("statType", StatType.ATK.Serialize())
                    .Add("value", 50m.Serialize())
                    .Add("additionalValue", 10m.Serialize()));
        var legacySkillDict = new Dictionary(new Dictionary<IKey, IValue>
        {
            [(Text)"skillRow"] = Dictionary.Empty
                .Add("id", 1)
                .Add("elemental_type", ElementalType.Fire.ToString())
                .Add("skill_type", SkillType.Attack.ToString())
                .Add("skill_category", SkillCategory.AreaAttack.ToString())
                .Add("skill_target_type", SkillTargetType.Enemy.ToString())
                .Add("hit_count", 1)
                .Add("cooldown", 10)
                .Add("combo", true),
            [(Text)"power"] = 100.Serialize(),
            [(Text)"chance"] = 10.Serialize(),
        });
        var legacyStatDict = new Dictionary(new[]
        {
            new KeyValuePair<IKey, IValue>((Text)"statType", StatType.HP.Serialize()),
            new KeyValuePair<IKey, IValue>((Text)"value", 100m.Serialize()),
        });
        var legacyDict = Dictionary.Empty
            .Add((Text)"id", 0.Serialize())
            .Add((Text)"grade", 1.Serialize())
            .Add((Text)"item_type", ItemType.Equipment.Serialize())
            .Add((Text)"item_sub_type", ItemSubType.Weapon.Serialize())
            .Add((Text)"elemental_type", ElementalType.Normal.Serialize())
            .Add((Text)"itemId", Guid.NewGuid().Serialize())
            .Add((Text)"statsMap", legacyStatsMapDict)
            .Add((Text)"skills", new List(legacySkillDict))
            .Add((Text)"buffSkills", new List())
            .Add((Text)"set_id", 2.Serialize())
            .Add((Text)"spine_resource_path", "path".Serialize())
            .Add((Text)"icon_id", 3)
            .Add((Text)"stat", legacyStatDict)
            .Add((Text)"requiredBlockIndex", 1000L.Serialize());

        var target = (Equipment) ItemFactory.Deserialize(legacyDict);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var deserialized = new Models.Items.Equipment(serialized);

        // Check Deserialize from List
        Assert.Equal(target.Id, deserialized.Id);
        Assert.Equal(target.ItemType, deserialized.ItemType);
        Assert.Equal(target.ItemSubType, deserialized.ItemSubType);
        Assert.Equal(target.Grade, deserialized.Grade);
        Assert.Equal(target.ElementalType, deserialized.ElementalType);
        Assert.Equal(target.Equipped, deserialized.Equipped);
        Assert.Equal(target.level, deserialized.Level);
        Assert.Equal(target.Exp, deserialized.Exp);
        Assert.Equal(target.SetId, deserialized.SetId);
        Assert.Equal(target.SpineResourcePath, deserialized.SpineResourcePath);
        Assert.Equal(target.optionCountFromCombination, deserialized.OptionCountFromCombination);
        Assert.Equal(target.MadeWithMimisbrunnrRecipe, deserialized.MadeWithMimisbrunnrRecipe);
        Assert.Equal(100m, deserialized.Stat.BaseValue);
        Assert.Equal(0m, deserialized.Stat.AdditionalValue);
        var statsMap = deserialized.StatsMap.Value;
        Assert.Equal(2352.7369m, statsMap[StatType.HP].BaseValue);
        Assert.Equal(0m, statsMap[StatType.HP].AdditionalValue);
        Assert.Equal(50m, statsMap[StatType.ATK].BaseValue);
        Assert.Equal(10m, statsMap[StatType.ATK].AdditionalValue);
        var skill = Assert.Single(deserialized.Skills);
        Assert.Equal(100, skill.Power);
        Assert.Equal(10, skill.Chance);
    }
}
