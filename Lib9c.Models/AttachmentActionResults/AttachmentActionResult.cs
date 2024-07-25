using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Item;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Costume = Lib9c.Models.Item.Costume;
using Item_Costume = Lib9c.Models.Item.Costume;
using Item_ItemUsable = Lib9c.Models.Item.ItemUsable;
using Item_TradableMaterial = Lib9c.Models.Item.TradableMaterial;
using ItemFactory = Lib9c.Models.Factories.ItemFactory;
using ItemUsable = Lib9c.Models.Item.ItemUsable;
using TradableMaterial = Lib9c.Models.Item.TradableMaterial;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.AttachmentActionResult"/>
/// </summary>
public record AttachmentActionResult : IBencodable
{
    public string TypeId { get; init; }
    public Item_ItemUsable? ItemUsable { get; init; }
    public Item_Costume? Costume { get; init; }

    /// <summary>
    /// The type of the <see cref="Nekoyume.Action.AttachmentActionResult.tradableFungibleItem"/> field in Lib9c
    /// that this property corresponds to is <see cref="ITradableFungibleItem"/>. However,
    /// only <see cref="Nekoyume.Model.Item.TradableMaterial"/> implements this type's interface in Lib9c,
    /// so we define it here as <see cref="Item.TradableMaterial"/> type that corresponding it.
    /// </summary>
    public Item_TradableMaterial? TradableFungibleItem { get; init; }

    public int TradableFungibleItemCount { get; init; }

    public virtual IValue Bencoded
    {
        get
        {
            var d = Dictionary.Empty.Add("typeId", TypeId.Serialize());
            if (ItemUsable is not null)
            {
                d = d.Add("itemUsable", ItemUsable.Bencoded);
            }

            if (Costume is not null)
            {
                d = d.Add("costume", Costume.Bencoded);
            }

            if (TradableFungibleItem is not null)
            {
                d = d
                    .Add("tradableFungibleItem", TradableFungibleItem.Bencoded)
                    .Add("tradableFungibleItemCount", TradableFungibleItemCount.Serialize());
            }

            return d;
        }
    }

    public AttachmentActionResult(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        TypeId = d["typeId"].ToDotnetString();
        ItemUsable = d.ContainsKey("itemUsable")
            ? (Item_ItemUsable)Factories.ItemFactory.Deserialize((Dictionary)d["itemUsable"])
            : null;
        Costume = d.ContainsKey("costume")
            ? (Item_Costume)Factories.ItemFactory.Deserialize((Dictionary)d["costume"])
            : null;
        TradableFungibleItem = d.ContainsKey("tradableFungibleItem")
            ? (Item_TradableMaterial)Factories.ItemFactory.Deserialize(
                (Dictionary)d["tradableFungibleItem"])
            : null;
        TradableFungibleItemCount = d.ContainsKey("tradableFungibleItemCount")
            ? d["tradableFungibleItemCount"].ToInteger()
            : default;
    }
}
