using Lib9c.Models.Skills;
using Lib9c.Models.Tests.Fixtures.Types.Skills;
using Nekoyume.Model.Stat;
using Nekoyume.TableData;

namespace Lib9c.Models.Tests.Skills;

public class SkillTest
{
    [Fact]
    public void Test()
    {
        // Prepare target state
        var row = new SkillSheet.Row();
        row.Set(new[] { "0", "Normal", "Attack", "NormalAttack", "Enemy", "1", "1", "false" });
        var target = new VanillaSkill(row, 99, 99, 99, StatType.HP);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new Skill(serialized);
        // ...

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);

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
