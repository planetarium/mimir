using System.Text.Json.Serialization;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.Items;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.Buy7.BuyerResult"/>
/// </summary>
[BsonIgnoreExtraElements]
public record SellCancellationResult : AttachmentActionResult
{
    public ShopItem ShopItem { get; init; }
    public Guid Id { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("shopItem", ShopItem.Bencoded)
        .Add("id", Id.Serialize());

    public SellCancellationResult()
    {
    }

    public SellCancellationResult(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        ShopItem = new ShopItem((Dictionary)d["shopItem"]);
        Id = d["id"].ToGuid();
    }
}
