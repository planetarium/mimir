using Bencodex.Types;
using Mimir.Models.Assets;
using Mimir.Models.Exceptions;
using Mimir.Models.Item;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Mimir.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.Buy7.SellerResult"/>
/// </summary>
public record Buy7SellerResult : AttachmentActionResult
{
    // FIXME to mimir's ShopItem
    public ShopItem ShopItem { get; init; }
    public Guid Id { get; init; }
    public FungibleAssetValue Gold { get; init; }

    public Buy7SellerResult(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        ShopItem = new ShopItem((Dictionary)d["shopItem"]);
        Id = d["id"].ToGuid();
        Gold = new FungibleAssetValue(d["gold"]);
    }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("shopItem", ShopItem.Bencoded)
        .Add("id", Id.Serialize())
        .Add("gold", Gold.Bencoded);
}
