using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.Factories;
using Lib9c.Models.Items;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Market;

/// <summary>
/// <see cref="Nekoyume.Model.Market.ItemProduct"/>
/// </summary>
[BsonIgnoreExtraElements]
public record ItemProduct : Product, IBencodable
{
    public ItemBase TradableItem { get; init; }
    public int ItemCount { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public new IValue Bencoded => ((List)base.Bencoded)
        .Add(TradableItem.Bencoded)
        .Add(ItemCount.Serialize());

    public ItemProduct()
    {
    }

    public ItemProduct(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        TradableItem = ItemFactory.Deserialize(l[6]);
        ItemCount = l[7].ToInteger();
    }
}
