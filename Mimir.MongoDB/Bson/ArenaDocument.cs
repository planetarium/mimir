using Libplanet.Crypto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.Arena;
using static Nekoyume.TableData.ArenaSheet;

namespace Mimir.MongoDB.Bson;

public record ArenaDocument(
    Address ScoreAddress,
    Address InformationAddress,
    ArenaInformation ArenaInformationObject,
    ArenaScore ArenaScoreObject,
    RoundData RoundData,
    Address AvatarAddress
) : MimirBsonDocument(ScoreAddress)
{
    [BsonExtraElements]
    public BsonDocument? ExtraElements { get; init; }
}
