using Nekoyume.Model;
using Nekoyume.Model.Buff;
using Nekoyume.Model.Skill;
using Nekoyume.Model.Stat;
using Nekoyume.TableData;

namespace Lib9c.Models.Tests.Fixtures.Types.Skills;

public class VanillaSkill : Skill
{
    public VanillaSkill(
        SkillSheet.Row skillRow,
        long power,
        int chance,
        int statPowerRatio,
        StatType referencedStatType) :
        base(skillRow, power, chance, statPowerRatio, referencedStatType)
    {
    }

    public override Nekoyume.Model.BattleStatus.Skill Use(
        CharacterBase caster,
        int simulatorWaveTurn,
        IEnumerable<Buff> buffs,
        bool copyCharacter)
    {
        throw new NotImplementedException();
    }
}
