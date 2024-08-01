using Libplanet.Crypto;
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
) : IMimirBsonDocument(ScoreAddress) { }
