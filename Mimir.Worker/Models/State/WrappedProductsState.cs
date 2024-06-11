using Libplanet.Crypto;
using Nekoyume.Model.Market;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class WrappedProductsState : State
{
    public ProductsState Object;

    public Address AvatarAddress;

    public WrappedProductsState(Address address, Address avatarAddress, ProductsState productsState)
        : base(address)
    {
        Object = productsState;
        AvatarAddress = avatarAddress;
    }
}
