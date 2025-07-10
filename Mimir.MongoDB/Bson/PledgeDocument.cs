using HotChocolate;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record PledgeDocument(
    [property: BsonIgnore, JsonIgnore, GraphQLIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore, GraphQLIgnore] Address Address,
    Address ContractAddress,
    bool Contracted,
    int RefillMead
) : MimirBsonDocument(Address.ToHex(), new DocumentMetadata(1, StoredBlockIndex));

