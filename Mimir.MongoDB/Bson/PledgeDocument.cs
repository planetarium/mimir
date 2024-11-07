using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record PledgeDocument(
    long StoredBlockIndex,
    Address Address,
    Address ContractAddress,
    bool Contracted,
    int RefillMead
) : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex)) { }
