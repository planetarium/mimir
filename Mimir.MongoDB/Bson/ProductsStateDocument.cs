using Lib9c.Models.Market;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record ProductsStateDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    ProductsState Object,
    Address AvatarAddress
) : MimirBsonDocument(Address, new DocumentMetadata(1, StoredBlockIndex));
