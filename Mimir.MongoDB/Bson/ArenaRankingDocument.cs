using Lib9c.Models.Arena;
using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record ArenaRankingDocument(
    long StoredBlockIndex,
    Address Address,
    int ChampionshipId,
    int Round,
    ArenaInformation ArenaInformation,
    ArenaScore ArenaScore,
    SimplifiedAvatarState SimpleAvatar
)
{
    public int Rank { get; set; }
}
