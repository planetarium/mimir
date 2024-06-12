using Libplanet.Crypto;
using Nekoyume.Model.Market;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class ProductState : State
{
    public Address AvatarAddress;
    public Address ProductsStateAddress;
    public Product Object;
    public int? CombatPoint;
    public decimal? UnitPrice;
    public int? Crystal;
    public int? CrystalPerPrice;

    public ProductState(
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

    public ProductState(
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
