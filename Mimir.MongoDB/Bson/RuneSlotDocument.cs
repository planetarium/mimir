using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record RuneSlotDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    Address AvatarAddress,
    RuneSlotState Object
) : MimirBsonDocument(Address.ToHex(), new DocumentMetadata(1, StoredBlockIndex));
