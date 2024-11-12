using Lib9c.Models.Arena;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

/// <param name="Address">Avatar address</param>
[BsonIgnoreExtraElements]
public record ArenaParticipantDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    ArenaParticipant Object
) : MimirBsonDocument(Address, new DocumentMetadata(ArenaParticipant.StateVersion, StoredBlockIndex));
