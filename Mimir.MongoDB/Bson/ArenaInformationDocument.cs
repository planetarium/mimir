using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.Arena;
using static Nekoyume.TableData.ArenaSheet;

namespace Mimir.MongoDB.Bson;

public record ArenaInformationDocument(
    ArenaInformation Object,
    RoundData RoundData,
    Address AvatarAddress)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
