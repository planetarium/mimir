using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.Arena;
using static Nekoyume.TableData.ArenaSheet;

namespace Mimir.MongoDB.Bson;

public record ArenaScoreDocument(
    ArenaScore Object,
    RoundData RoundData,
    Address AvatarAddress)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
