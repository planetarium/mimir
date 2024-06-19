using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.Models.Product;

[BsonIgnoreExtraElements]
public class ItemProductObject : IProductObject
{
    public TradableItem TradableItem { get; set; }
    public int ItemCount { get; set; }
    public string ProductId { get; set; }
    public int Type { get; set; }
    public Price Price { get; set; }
    public long RegisteredBlockIndex { get; set; }
    public string SellerAvatarAddress { get; set; }
    public string SellerAgentAddress { get; set; }
}
