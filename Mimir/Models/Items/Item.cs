using MongoDB.Bson;
using Nekoyume.Model.Elemental;
using Nekoyume.Model.Item;

namespace Mimir.Models.Items;

public class Item
{
    public int ItemSheetId { get; set; }
    public int Grade { get; set; }
    public ItemType ItemType { get; set; }
    public ItemSubType ItemSubType { get; set; }
    public ElementalType ElementalType { get; set; }

    public int Count { get; set; }

    public Guid? NonFungibleId { get; set; }
    public long? RequiredBlockIndex { get; set; }

    public Item(ItemBase item, int count)
    {
        ItemSheetId = item.Id;
        Grade = item.Grade;
        ItemType = item.ItemType;
        ItemSubType = item.ItemSubType;
        ElementalType = item.ElementalType;

        Count = count;

        NonFungibleId = item is INonFungibleItem nonFungibleItem
            ? nonFungibleItem.NonFungibleId
            : null;
        RequiredBlockIndex = item switch
        {
            INonFungibleItem nfi => nfi.RequiredBlockIndex,
            ITradableItem ti => ti.RequiredBlockIndex,
            _ => null
        };
    }

    public Item(BsonDocument item)
    {
        ItemSheetId = item["ItemSheetId"].AsInt32;
        Grade = item["Grade"].AsInt32;
        ItemType = (ItemType)item["ItemType"].AsInt32;
        ItemSubType = (ItemSubType)item["ItemSubType"].AsInt32;
        ElementalType = (ElementalType)item["ElementalType"].AsInt32;

        Count = item["Count"].AsInt32;

        NonFungibleId = item["NonFungibleId"].AsNullableGuid;
        RequiredBlockIndex = item["RequiredBlockIndex"].AsNullableInt64;
    }
}
