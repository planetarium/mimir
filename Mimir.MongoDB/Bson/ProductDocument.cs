using Lib9c.Models.Market;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record ProductDocument : MimirBsonDocument
{
    public Address AvatarAddress { get; init; }
    public Address ProductsStateAddress { get; init; }
    public Product Object { get; init; }
    public int? CombatPoint { get; init; }
    public decimal? UnitPrice { get; init; }
    public int? Crystal { get; init; }
    public int? CrystalPerPrice { get; init; }

    public ProductDocument(
        long StoredBlockIndex,
        Address address,
        Address avatarAddress,
        Address productsStateAddress,
        Product product
    )
        : base(address, new DocumentMetadata(1, StoredBlockIndex))
    {
        Object = product;
        AvatarAddress = avatarAddress;
        ProductsStateAddress = productsStateAddress;
    }

    public ProductDocument(
        long StoredBlockIndex,
        Address address,
        Address avatarAddress,
        Address productsStateAddress,
        Product product,
        decimal unitPrice,
        int? combatPoint,
        int? crystal,
        int? crystalPerPrice
    )
        : base(address, new DocumentMetadata(1, StoredBlockIndex))
    {
        Object = product;
        AvatarAddress = avatarAddress;
        ProductsStateAddress = productsStateAddress;
        CombatPoint = combatPoint;
        UnitPrice = unitPrice;
        Crystal = crystal;
        CrystalPerPrice = crystalPerPrice;
    }
}
