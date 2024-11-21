using Lib9c.Models.Arena;
using Lib9c.Models.States;
using Libplanet.Crypto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

/// <param name="Address">Avatar address</param>
[BsonIgnoreExtraElements]
public record ArenaDocument(
    [property: BsonIgnore, JsonIgnore] long StoredBlockIndex,
    [property: BsonIgnore, JsonIgnore] Address Address,
    int ChampionshipId,
    int Round,
    ArenaInformation ArenaInformation,
    ArenaScore ArenaScore,
    SimplifiedAvatarState SimpleAvatar
) : MimirBsonDocument(Address.ToHex(), new DocumentMetadata(1, StoredBlockIndex))
{
    [BsonIgnore, JsonIgnore]
    public Address AvatarAddress => Address;

    [BsonExtraElements]
    public BsonDocument? ExtraElements { get; init; }
}
