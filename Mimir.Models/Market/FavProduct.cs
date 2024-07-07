using Bencodex;
using Bencodex.Types;
using Libplanet.Types.Assets;
using Nekoyume.Model.State;

namespace Mimir.Models.Market;

public class FavProduct : Product, IBencodable
{
    public FungibleAssetValue Asset { get; private set; }

    public FavProduct(List bencoded)
        : base(bencoded)
    {
        Asset = bencoded[6].ToFungibleAssetValue();
    }

    [GraphQLIgnore]
    public IValue Bencoded => Serialize();

    public IValue Serialize()
    {
        List serialized = (List)base.Bencoded;
        return serialized.Add(Asset.Serialize());
    }
}
