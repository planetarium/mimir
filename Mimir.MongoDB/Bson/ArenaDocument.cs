using Lib9c.Models.Arena;
using Libplanet.Crypto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

/// <param name="Address">Avatar address</param>
[BsonIgnoreExtraElements]
public record ArenaDocument(
    Address Address,
    Nekoyume.TableData.ArenaSheet.RoundData RoundData,
    ArenaInformation ArenaInformation,
    ArenaScore ArenaScore)
    : MimirBsonDocument(Address)
{
    [BsonIgnore]
    public Address AvatarAddress => Address;

    [BsonExtraElements]
    public BsonDocument? ExtraElements { get; init; }
}
