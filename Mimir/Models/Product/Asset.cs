namespace Mimir.Models.Product;

public class Asset
{
    public Currency Currency { get; set; }
    public string RawValue { get; set; }
    public int Sign { get; set; }
    public string MajorUnit { get; set; }
    public string MinorUnit { get; set; }
}
