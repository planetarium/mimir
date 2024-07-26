using Bencodex;
using Bencodex.Types;
using Libplanet.Types.Assets;

namespace Mimir.MongoDB.Bson;

public class BalanceState(FungibleAssetValue value) : IBencodable
{
    public FungibleAssetValue Object { get; set; } = value;

    public IValue Bencoded => Object.Serialize();
}
