using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using NCProductsState = Nekoyume.Model.Market.ProductsState;

namespace Mimir.MongoDB.Bson;

public class ProductsState : IBencodable
{
    public NCProductsState Object;

    public Address AvatarAddress;

    public ProductsState(Address avatarAddress, NCProductsState productsState)
    {
        Object = productsState;
        AvatarAddress = avatarAddress;
    }

    public IValue Bencoded => Object.Serialize();
}
