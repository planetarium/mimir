using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.Market;

namespace Mimir.MongoDB.Bson;

public record ProductDocument : IMimirBsonDocument
{
    public Address Address { get; init; }
    public Product Object { get; init; }
    public Address AvatarAddress { get; init; }
    public Address ProductsStateAddress { get; init; }
    public int? CombatPoint { get; init; }
    public decimal? UnitPrice { get; init; }
    public int? Crystal { get; init; }
    public int? CrystalPerPrice { get; init; }

    public ProductDocument(
        Address address,
        Product product,
        Address avatarAddress,
        Address productsStateAddress)
    {
        Address = address;
        Object = product;
        AvatarAddress = avatarAddress;
        ProductsStateAddress = productsStateAddress;
    }

    public ProductDocument(
        Address address,
        Product product,
        Address avatarAddress,
        Address productsStateAddress,
        decimal unitPrice,
        int? combatPoint,
        int? crystal,
        int? crystalPerPrice)
    {
        Address = address;
        Object = product;
        AvatarAddress = avatarAddress;
        ProductsStateAddress = productsStateAddress;
        CombatPoint = combatPoint;
        UnitPrice = unitPrice;
        Crystal = crystal;
        CrystalPerPrice = crystalPerPrice;
    }

    public IValue Bencoded => Object.Serialize();
}
