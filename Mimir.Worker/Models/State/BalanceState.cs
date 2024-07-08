using Bencodex.Types;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class BalanceState(Address address, FungibleAssetValue value) : State(address)
{
    public FungibleAssetValue Object { get; set; } = value;

    public override IValue Serialize()
    {
        return Object.Serialize();
    }
}
