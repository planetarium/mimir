using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record PledgeDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    Address ContractAddress,
    bool Contracted,
    int RefillMead
) : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex)) { }
