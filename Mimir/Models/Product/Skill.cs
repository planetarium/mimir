namespace Mimir.Models.Product;

public class Skill
{
    public SkillRow SkillRow { get; set; }
    public int Power { get; set; }
    public int Chance { get; set; }
    public int StatPowerRatio { get; set; }
    public int ReferencedStatType { get; set; }
}
