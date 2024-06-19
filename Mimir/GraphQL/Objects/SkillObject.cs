using Nekoyume.Model.Stat;
using Nekoyume.TableData;

namespace Mimir.GraphQL.Objects;

public class SkillObject(
    SkillSheet.Row skillRow,
    long power,
    int chance,
    int statPowerRatio,
    StatType referencedStatType)
{
    public SkillSheet.Row SkillRow { get; set; } = skillRow;
    public long Power { get; set; } = power;
    public int Chance { get; set; } = chance;
    public int StatPowerRatio { get; set; } = statPowerRatio;
    public StatType ReferencedStatType { get; set; } = referencedStatType;
}
