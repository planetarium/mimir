using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record RuneSlotDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address AvatarAddress,
    Address RuneSlotAddress,
    RuneSlotState Object
) : MimirBsonDocument(AvatarAddress.ToHex(), new DocumentMetadata(1, StoredBlockIndex));
