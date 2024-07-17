using Bencodex;
using Bencodex.Types;
using Libplanet.Types.Assets;

namespace Mimir.Worker.Models;

public class BalanceState(FungibleAssetValue value) : IBencodable
{
    public FungibleAssetValue Object { get; set; } = value;

    public IValue Bencoded => Object.Serialize();
}
