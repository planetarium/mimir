namespace Mimir.Models.Product;

public class Currency
{
    public string Ticker { get; set; }
    public int DecimalPlaces { get; set; }
    public string Hash { get; set; }
    public bool TotalSupplyTrackable { get; set; }
}
