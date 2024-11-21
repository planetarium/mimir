using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record WorldBossKillRewardRecordDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    Address AvatarAddress,
    WorldBossKillRewardRecord Object
) : MimirBsonDocument(Address.ToHex(), new DocumentMetadata(1, StoredBlockIndex));
