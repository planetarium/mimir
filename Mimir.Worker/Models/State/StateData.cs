using Bencodex;
using Libplanet.Crypto;

namespace Mimir.Worker.Models;

public record StateData(Address Address, IBencodable State) : BaseData;
