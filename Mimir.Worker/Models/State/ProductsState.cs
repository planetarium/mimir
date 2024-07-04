using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.State;
using NCProductsState = Nekoyume.Model.Market.ProductsState;

namespace Mimir.Worker.Models;

public class ProductsState : State
{
    public NCProductsState Object;

    public Address AvatarAddress;

    public ProductsState(Address address, Address avatarAddress, NCProductsState productsState)
        : base(address)
    {
        Object = productsState;
        AvatarAddress = avatarAddress;
    }

    public IValue Serialize()
    {
        return Object.Serialize();
    }
}
