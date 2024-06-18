using Mimir.Models.Product;

public interface IProductObject
{
    public string ProductId { get; set; }
    public int Type { get; set; }
    public Price Price { get; set; }
    public long RegisteredBlockIndex { get; set; }
    public string SellerAvatarAddress { get; set; }
    public string SellerAgentAddress { get; set; }
}
