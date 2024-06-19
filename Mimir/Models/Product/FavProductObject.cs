using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.Models.Product;

[BsonIgnoreExtraElements]
public class FavProductObject : IProductObject
{
    public Asset Asset { get; set; }
    public string ProductId { get; set; }
    public int Type { get; set; }
    public Price Price { get; set; }
    public long RegisteredBlockIndex { get; set; }
    public string SellerAvatarAddress { get; set; }
    public string SellerAgentAddress { get; set; }
}
