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
}
