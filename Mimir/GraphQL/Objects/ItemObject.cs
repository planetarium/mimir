using System.Security.Cryptography;
using Libplanet.Common;
using Nekoyume.Model.Elemental;
using Nekoyume.Model.Item;

namespace Mimir.GraphQL.Objects;

public class ItemObject
{
    public int ItemSheetId { get; set; }
    public int Grade { get; set; }
    public ItemType ItemType { get; set; }
    public ItemSubType ItemSubType { get; set; }
    public ElementalType ElementalType { get; set; }

    public int Count { get; set; }

    /// <summary>
    /// Level of the Equipment.
    /// </summary>
    public int? Level { get; set; }

    public long? RequiredBlockIndex { get; set; }

    /// <summary>
    /// ID of the IFungibleItem.
    /// </summary>
    public HashDigest<SHA256>? FungibleId { get; set; }

    /// <summary>
    /// ID of the INonFungibleItem.
    /// </summary>
    public Guid? NonFungibleId { get; set; }

    /// <summary>
    /// ID of the TradableItem.
    /// </summary>
    public Guid? TradableId { get; set; }

    public ItemObject()
    {
    }

    public ItemObject(ItemBase itemBase, int count)
    {
        ItemSheetId = itemBase.Id;
        Grade = itemBase.Grade;
        ItemType = itemBase.ItemType;
        ItemSubType = itemBase.ItemSubType;
        ElementalType = itemBase.ElementalType;
        Count = count;
    }

    public ItemObject(Consumable consumable, int count) : this((ItemBase)consumable, count)
    {
        Level = null;
        RequiredBlockIndex = consumable.RequiredBlockIndex;
        FungibleId = null;
        NonFungibleId = consumable.NonFungibleId;
        TradableId = consumable.TradableId;
    }

    public ItemObject(Costume costume, int count) : this((ItemBase)costume, count)
    {
        Level = null;
        RequiredBlockIndex = costume.RequiredBlockIndex;
        FungibleId = null;
        NonFungibleId = costume.NonFungibleId;
        TradableId = costume.TradableId;
    }

    public ItemObject(Equipment equipment, int count) : this((ItemBase)equipment, count)
    {
        var tradableItem = equipment as ITradableItem;

        Level = equipment.level;
        RequiredBlockIndex = equipment.RequiredBlockIndex;
        FungibleId = null;
        NonFungibleId = equipment.NonFungibleId;
        TradableId = tradableItem?.TradableId;
    }

    public ItemObject(Material material, int count) : this((ItemBase)material, count)
    {
        var tradableItem = material as ITradableItem;

        Level = null;
        RequiredBlockIndex = tradableItem?.RequiredBlockIndex;
        FungibleId = material.FungibleId;
        NonFungibleId = null;
        TradableId = tradableItem?.TradableId;
    }
}
