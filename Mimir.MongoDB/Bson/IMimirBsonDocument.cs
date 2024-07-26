using Bencodex;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public interface IMimirBsonDocument : IBencodable
{
    Address Address { get; }
}
