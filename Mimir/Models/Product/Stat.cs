namespace Mimir.Models.Product;

public class Stat
{
    public int StatType { get; set; }
    public int BaseValue { get; set; }
    public int AdditionalValue { get; set; }
    public bool HasTotalValueAsLong { get; set; }
    public bool HasBaseValueAsLong { get; set; }
    public bool HasAdditionalValueAsLong { get; set; }
    public bool HasBaseValue { get; set; }
    public bool HasAdditionalValue { get; set; }
    public long BaseValueAsLong { get; set; }
    public long AdditionalValueAsLong { get; set; }
    public long TotalValueAsLong { get; set; }
    public int TotalValue { get; set; }
}
