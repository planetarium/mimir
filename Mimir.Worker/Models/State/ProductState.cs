using Libplanet.Crypto;
using Nekoyume.Model.Market;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class ProductState : State
{
    public Address AvatarAddress;
    public Address ProductsStateAddress;
    public Product Object;

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
}
