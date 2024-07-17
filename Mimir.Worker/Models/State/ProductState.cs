using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.Market;

namespace Mimir.Worker.Models;

public class ProductState : IBencodable
{
    public Address AvatarAddress;
    public Address ProductsStateAddress;
    public Product Object;
    public int? CombatPoint;
    public decimal? UnitPrice;
    public int? Crystal;
    public int? CrystalPerPrice;

    public ProductState(
        Address avatarAddress,
        Address productsStateAddress,
        Product product
    )
    {
        Object = product;
        AvatarAddress = avatarAddress;
        ProductsStateAddress = productsStateAddress;
    }

    public ProductState(
        Address avatarAddress,
        Address productsStateAddress,
        Product product,
        decimal unitPrice,
        int? combatPoint,
        int? crystal,
        int? crystalPerPrice
    )
    {
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
