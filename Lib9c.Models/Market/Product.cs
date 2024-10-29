using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Market;

/// <summary>
/// <see cref="Nekoyume.Model.Market.Product"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Product : IBencodable
{
    public Guid ProductId { get; init; }
    public Nekoyume.Model.Market.ProductType ProductType { get; init; }
    public FungibleAssetValue Price { get; init; }
    public long RegisteredBlockIndex { get; init; }
    public Address SellerAvatarAddress { get; init; }
    public Address SellerAgentAddress { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded => List.Empty
        .Add(ProductId.Serialize())
        .Add(ProductType.Serialize())
        .Add(Price.Serialize())
        .Add(RegisteredBlockIndex.Serialize())
        .Add(SellerAgentAddress.Serialize())
        .Add(SellerAvatarAddress.Serialize());

    public Product()
    {
    }

    public Product(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        ProductId = l[0].ToGuid();
        ProductType = l[1].ToEnum<Nekoyume.Model.Market.ProductType>();
        Price = l[2].ToFungibleAssetValue();
        RegisteredBlockIndex = l[3].ToLong();
        SellerAgentAddress = l[4].ToAddress();
        SellerAvatarAddress = l[5].ToAddress();
    }
}
