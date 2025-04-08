using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record AvatarDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    AvatarState Object,
    int? ArmorId,
    int? PortraitId
) : MimirBsonDocument(Address.ToHex(), new DocumentMetadata(2, StoredBlockIndex));
