using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Extensions;
using Libplanet.Types.Assets;

namespace Lib9c.Models.Market;

public class FavProduct : Product, IBencodable
{
    public FungibleAssetValue Asset { get; }

    public FavProduct(List bencoded)
        : base(bencoded)
    {
        Asset = bencoded[6].ToFungibleAssetValue();
    }

    [GraphQLIgnore]
    public new IValue Bencoded => Serialize();

    public IValue Serialize()
    {
        List serialized = (List)base.Bencoded;
        return serialized.Add(Asset.Serialize());
    }
}
