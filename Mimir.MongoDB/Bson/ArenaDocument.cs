using Lib9c.Models.Arena;
using Libplanet.Crypto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

/// <param name="Address">Avatar address</param>
[BsonIgnoreExtraElements]
public record ArenaDocument(
    Address Address,
    int ChampionshipId,
    int Round,
    ArenaInformation ArenaInformation,
    ArenaScore ArenaScore)
    : MimirBsonDocument(Address)
{
    [BsonIgnore, JsonIgnore]
    public Address AvatarAddress => Address;

    [BsonExtraElements]
    public BsonDocument? ExtraElements { get; init; }
}
