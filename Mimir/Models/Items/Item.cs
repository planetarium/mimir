using MongoDB.Bson;
using Nekoyume.Model.Item;

namespace Mimir.Models.Items;

public class Item
{
    public ItemSubType ItemSubType { get; set; }

    public Item(IItem item)
    {
        ItemSubType = item.ItemSubType;
    }

    public Item(BsonValue item)
    {
        ItemSubType = (ItemSubType)item["ItemSubType"].AsInt32;
    }
}
