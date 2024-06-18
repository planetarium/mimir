using System.Collections.Generic;

namespace Mimir.Models.Product;

public class TradableItem
{
    public bool Equipped { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int OptionCountFromCombination { get; set; }
    public string TradableId { get; set; }
    public Stat Stat { get; set; }
    public int SetId { get; set; }
    public string SpineResourcePath { get; set; }
    public bool MadeWithMimisbrunnrRecipe { get; set; }
    public int UniqueStatType { get; set; }
    public string ItemId { get; set; }
    public string NonFungibleId { get; set; }
    public Dictionary<string, int> StatsMap { get; set; }
    public List<Skill> Skills { get; set; }
    public List<Skill> BuffSkills { get; set; }
    public long RequiredBlockIndex { get; set; }
    public int Id { get; set; }
    public int Grade { get; set; }
    public int ItemType { get; set; }
    public int ItemSubType { get; set; }
    public int ElementalType { get; set; }
}
