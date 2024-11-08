using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record ActionPointDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    int Object
) : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
