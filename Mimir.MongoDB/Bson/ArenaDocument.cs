using System.Text.Json.Serialization;
using Lib9c.Models.Arena;
using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

/// <param name="Address">Avatar address</param>
[BsonIgnoreExtraElements]
public record ArenaDocument(
    Address Address,
    int ChampionshipId,
    int Round,
    ArenaInformation ArenaInformation,
    ArenaScore ArenaScore,
    SimplifiedAvatarState SimpleAvatar)
    : MimirBsonDocument(Address)
{
    [BsonIgnore, JsonIgnore]
    public Address AvatarAddress => Address;

    [BsonExtraElements]
    public BsonDocument? ExtraElements { get; init; }
}
