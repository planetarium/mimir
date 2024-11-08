using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record BalanceDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    string Object
) : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
