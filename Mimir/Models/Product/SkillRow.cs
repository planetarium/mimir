namespace Mimir.Models.Product;

public class SkillRow
{
    public int Key { get; set; }
    public int Id { get; set; }
    public int ElementalType { get; set; }
    public int SkillType { get; set; }
    public int SkillCategory { get; set; }
    public int SkillTargetType { get; set; }
    public int HitCount { get; set; }
    public int Cooldown { get; set; }
    public bool Combo { get; set; }
}
