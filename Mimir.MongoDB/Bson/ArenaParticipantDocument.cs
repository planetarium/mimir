using Lib9c.Models.Arena;
using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

/// <param name="Address">Avatar address</param>
[BsonIgnoreExtraElements]
public record ArenaParticipantDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    ArenaParticipant Object,
    int ChampionshipId,
    int Round,
    SimplifiedAvatarState SimpleAvatar) :
    MimirBsonDocument(
        $"{Address.ToHex()}_{ChampionshipId}_{Round}",
        new DocumentMetadata(ArenaParticipant.StateVersion, StoredBlockIndex))
{
    [BsonIgnore, JsonIgnore]
    public int Rank { get; set; }
}
