using Libplanet.Crypto;
using Nekoyume.Model.Market;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class WrappedProductsState : State
{
    public ProductsState Object;

    public WrappedProductsState(Address address, ProductsState productsState)
        : base(address)
    {
        Object = productsState;
    }
}
