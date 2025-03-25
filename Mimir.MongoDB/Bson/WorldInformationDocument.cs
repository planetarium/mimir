using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record WorldInformationDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    int LastStageClearedId,
    WorldInformationState Object
) : MimirBsonDocument(Address.ToHex(), new DocumentMetadata(2, StoredBlockIndex));
