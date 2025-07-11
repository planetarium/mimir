using Bencodex.Types;
using Lib9c.Models.Tests.Fixtures.Types.Skills;
using Nekoyume.Model.Elemental;
using Nekoyume.Model.Skill;
using Nekoyume.Model.Stat;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using Skill = Lib9c.Models.Skills.Skill;

namespace Lib9c.Models.Tests.Skills;

public class SkillTest
{
    [Fact]
    public void Test()
    {
        // Prepare target state
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
            [(Text)"stat_power_ratio"] = 50.Serialize(),
            [(Text)"referenced_stat_type"] = StatType.ATK.Serialize(),
        });
        var target = SkillFactory.Deserialize(legacySkillDict);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new Skill(serialized);
        Assert.Equal(100, paired.Power);
        Assert.Equal(10, paired.Chance);
        Assert.Equal(50, paired.StatPowerRatio);
        Assert.Equal(StatType.ATK, paired.ReferencedStatType);

        // deserialize bencoded state as target2 and verify
        var target2 = new VanillaSkill(
            paired.SkillRow,
            paired.Power,
            paired.Chance,
            paired.StatPowerRatio,
            paired.ReferencedStatType);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
