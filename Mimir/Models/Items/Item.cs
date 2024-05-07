using MongoDB.Bson;
using Nekoyume.Model.Item;

namespace Mimir.Models.Items;

public class Item(BsonValue item)
{
    public ItemSubType ItemSubType { get; set; } = (ItemSubType)item["ItemSubType"].AsInt32;
}
