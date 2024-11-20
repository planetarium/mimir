using Mimir.MongoDB.Enums;
using Nekoyume.Model.Item;
using Nekoyume.Model.Market;

namespace Mimir.MongoDB.Models;

public class ProductFilter
{
    public ProductType? ProductType { get; set; }
    public ItemType? ItemType { get; set; }
    public ItemSubType? ItemSubType { get; set; }
    public ProductSortBy? SortBy { get; set; }
    public SortDirection? SortDirection { get; set; } = Enums.SortDirection.Ascending;
}