using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Libplanet.Types.Assets;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Market;

/// <summary>
/// <see cref="Nekoyume.Model.Market.FavProduct"/>
/// </summary>
[BsonIgnoreExtraElements]
public record FavProduct : Product, IBencodable
{
    public FungibleAssetValue Asset { get; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public new IValue Bencoded => ((List)base.Bencoded)
        .Add(Asset.Serialize());

    public FavProduct(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        Asset = l[6].ToFungibleAssetValue();
    }
}
