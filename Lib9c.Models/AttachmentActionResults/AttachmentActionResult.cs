using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.Factories;
using Lib9c.Models.Items;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.AttachmentActionResult"/>
/// </summary>
[BsonIgnoreExtraElements]
public record AttachmentActionResult : IBencodable
{
    public string TypeId { get; init; }
    public ItemUsable? ItemUsable { get; init; }
    public Costume? Costume { get; init; }

    /// <summary>
    /// The type of the <see cref="Nekoyume.Action.AttachmentActionResult.tradableFungibleItem"/> field in Lib9c
    /// that this property corresponds to is <see cref="Nekoyume.Model.Item.ITradableFungibleItem"/>. However,
    /// only <see cref="Nekoyume.Model.Item.TradableMaterial"/> implements this type's interface in Lib9c,
    /// so we define it here as <see cref="Items.TradableMaterial"/> type that corresponding it.
    /// </summary>
    public TradableMaterial? TradableFungibleItem { get; init; }

    public int TradableFungibleItemCount { get; init; }

    [BsonIgnore, GraphQLIgnore]
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

    public AttachmentActionResult()
    {
    }

    public AttachmentActionResult(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        TypeId = d["typeId"].ToDotnetString();
        ItemUsable = d.ContainsKey("itemUsable")
            ? (ItemUsable)ItemFactory.Deserialize(d["itemUsable"])
            : null;
        Costume = d.ContainsKey("costume")
            ? (Costume)ItemFactory.Deserialize(d["costume"])
            : null;
        TradableFungibleItem = d.ContainsKey("tradableFungibleItem")
            ? (TradableMaterial)ItemFactory.Deserialize(
                d["tradableFungibleItem"])
            : null;
        TradableFungibleItemCount = d.ContainsKey("tradableFungibleItemCount")
            ? d["tradableFungibleItemCount"].ToInteger()
            : default;
    }
}
