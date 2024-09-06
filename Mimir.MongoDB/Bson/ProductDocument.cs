using Lib9c.Models.Market;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

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
        Address address,
        Address avatarAddress,
        Address productsStateAddress,
        Product product
    )
        : base(address)
    {
        Object = product;
        AvatarAddress = avatarAddress;
        ProductsStateAddress = productsStateAddress;
    }

    public ProductDocument(
        Address address,
        Address avatarAddress,
        Address productsStateAddress,
        Product product,
        decimal unitPrice,
        int? combatPoint,
        int? crystal,
        int? crystalPerPrice
    )
        : base(address)
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
