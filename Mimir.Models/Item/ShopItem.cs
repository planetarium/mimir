using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.Models.Exceptions;
using Mimir.Models.Factories;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;

namespace Mimir.Models.Item;

public record ShopItem : IBencodable
{
    public Address SellerAgentAddress { get; init; }
    public Address SellerAvatarAddress { get; init; }
    public Guid ProductId { get; init; }
    public FungibleAssetValue Price { get; init; }
    public ItemUsable? ItemUsable { get; init; }
    public Costume? Costume { get; init; }

    /// <summary>
    /// The type of the <see cref="Nekoyume.Action.AttachmentActionResult.tradableFungibleItem"/> field in Lib9c
    /// that this property corresponds to is <see cref="ITradableFungibleItem"/>. However,
    /// only <see cref="Nekoyume.Model.Item.TradableMaterial"/> implements this type's interface in Lib9c,
    /// so we define it here as <see cref="TradableMaterial"/> type that corresponding it.
    /// </summary>
    public TradableMaterial? TradableFungibleItem { get; init; }

    public int TradableFungibleItemCount { get; init; }
    public long ExpiredBlockIndex { get; init; }

    public ShopItem(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        SellerAgentAddress = d[LegacySellerAgentAddressKey].ToAddress();
        SellerAvatarAddress = d[LegacySellerAvatarAddressKey].ToAddress();
        ProductId = d[LegacyProductIdKey].ToGuid();
        Price = d[LegacyPriceKey].ToFungibleAssetValue();
        ItemUsable = d.ContainsKey(LegacyItemUsableKey)
            ? (ItemUsable)ItemFactory.Deserialize((Dictionary)d[LegacyItemUsableKey])
            : null;
        Costume = d.ContainsKey(LegacyCostumeKey)
            ? (Costume)ItemFactory.Deserialize((Dictionary)d[LegacyCostumeKey])
            : null;
        TradableFungibleItem = d.ContainsKey(TradableFungibleItemKey)
            ? (TradableMaterial)ItemFactory.Deserialize((Dictionary)d[TradableFungibleItemKey])
            : null;
        TradableFungibleItemCount = d.ContainsKey(TradableFungibleItemCountKey)
            ? d[TradableFungibleItemCountKey].ToInteger()
            : default;
        if (d.ContainsKey(ExpiredBlockIndexKey))
        {
            ExpiredBlockIndex = d[ExpiredBlockIndexKey].ToLong();
        }
    }

    public IValue Bencoded
    {
        get
        {
            var d = Dictionary.Empty
                .Add(LegacySellerAgentAddressKey, SellerAgentAddress.Serialize())
                .Add(LegacySellerAvatarAddressKey, SellerAvatarAddress.Serialize())
                .Add(LegacyProductIdKey, ProductId.Serialize())
                .Add(LegacyPriceKey, Price.Serialize());

            if (ItemUsable is not null)
            {
                d = d.Add(LegacyItemUsableKey, ItemUsable.Bencoded);
            }

            if (Costume is not null)
            {
                d = d.Add(LegacyCostumeKey, Costume.Bencoded);
            }

            if (TradableFungibleItem is not null)
            {
                d = d.Add(TradableFungibleItemKey, TradableFungibleItem.Bencoded);
            }

            if (TradableFungibleItemCount != 0)
            {
                d = d.Add(TradableFungibleItemCountKey, TradableFungibleItemCount.Serialize());
            }

            if (ExpiredBlockIndex != 0)
            {
                d = d.Add(ExpiredBlockIndexKey, ExpiredBlockIndex.Serialize());
            }

            return d;
        }
    }
}
