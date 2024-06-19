using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.Models.Product;

[BsonIgnoreExtraElements]
public class Stat
{
    public int StatType { get; set; }
    public int BaseValue { get; set; }
    public int AdditionalValue { get; set; }
    public int TotalValue { get; set; }
}
