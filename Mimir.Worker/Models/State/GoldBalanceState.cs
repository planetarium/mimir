using System.Numerics;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class GoldBalanceState(Address address, BigInteger value) : State(address)
{
    public BigInteger Object { get; set; } = value;
}
