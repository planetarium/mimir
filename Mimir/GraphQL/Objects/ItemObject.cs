using System.Security.Cryptography;
using Libplanet.Common;
using Nekoyume.Model.Elemental;
using Nekoyume.Model.Item;
using Nekoyume.Model.Stat;

namespace Mimir.GraphQL.Objects;

public class ItemObject
{
    public int ItemSheetId { get; set; }
    public int Grade { get; set; }
    public ItemType ItemType { get; set; }
    public ItemSubType ItemSubType { get; set; }
    public ElementalType ElementalType { get; set; }

    /// <summary>
    /// Count of the Item.<seealso cref="Nekoyume.Model.Item.Inventory.Item.count"/>
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Locked status of the item.<seealso cref="Nekoyume.Model.Item.Inventory.Item.Locked"/>
    /// </summary>
    public bool Locked { get; set; }

    /// <summary>
    /// Level of the Equipment.
    /// </summary>
    public int? Level { get; set; }

    /// <summary>
    /// Exp of the Equipment.
    /// </summary>
    public long? Exp { get; set; }

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

    /// <summary>
    /// Equipped status of the IEquippableItem.
    /// </summary>
    public bool? Equipped { get; set; }

    /// <summary>
    /// MainStatType of the Consumable or Equipment.
    /// </summary>
    public StatType? MainStatType { get; set; }

    /// <summary>
    /// StatsMap of the ItemUsable.
    /// </summary>
    public StatMap? StatsMap { get; set; }
}
