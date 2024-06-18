namespace Mimir.Models.Product;

public class ProductState
{
    public string AvatarAddress { get; set; }
    public string ProductsStateAddress { get; set; }
    public IProductObject Object { get; set; }
}
