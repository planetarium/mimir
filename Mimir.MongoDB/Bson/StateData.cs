using Bencodex;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record StateData(Address Address, IBencodable State) : BaseData;
