using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record PledgeDocument(Address Address, Address ContractAddress, bool Contracted, int RefillMead)
    : MimirBsonDocument(Address)
{
}
