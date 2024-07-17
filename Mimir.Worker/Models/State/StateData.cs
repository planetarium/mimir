using Bencodex;
using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public record StateData(Address Address, IBencodable State) : BaseData;
