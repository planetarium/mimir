using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.Models.Product;

[BsonIgnoreExtraElements]
public class TradableItem
{
    [BsonElement("Id")]
    public int Id { get; set; }

    [BsonElement("level")]
    public int Level { get; set; }

    [BsonElement("Exp")]
    public int Exp { get; set; }

    [BsonElement("optionCountFromCombination")]
    public int OptionCountFromCombination { get; set; }

    [BsonElement("TradableId")]
    public string TradableId { get; set; }

    [BsonElement("Stat")]
    public Stat Stat { get; set; }

    [BsonElement("SetId")]
    public int SetId { get; set; }

    [BsonElement("UniqueStatType")]
    public int UniqueStatType { get; set; }

    [BsonElement("ItemId")]
    public string ItemId { get; set; }

    [BsonElement("NonFungibleId")]
    public string NonFungibleId { get; set; }

    [BsonElement("StatsMap")]
    public Dictionary<string, int> StatsMap { get; set; }

    [BsonElement("RequiredBlockIndex")]
    public long RequiredBlockIndex { get; set; }

    [BsonElement("Grade")]
    public int Grade { get; set; }

    [BsonElement("ItemType")]
    public int ItemType { get; set; }

    [BsonElement("ItemSubType")]
    public int ItemSubType { get; set; }

    [BsonElement("ElementalType")]
    public int ElementalType { get; set; }
}
