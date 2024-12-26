using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record ItemSlotDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    Address AvatarAddress,
    ItemSlotState Object
) : MimirBsonDocument(Address.ToHex(), new DocumentMetadata(2, StoredBlockIndex));
