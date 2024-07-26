using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.Arena;
using static Nekoyume.TableData.ArenaSheet;

namespace Mimir.MongoDB.Bson;

public class ArenaInformationState : IBencodable
{
    public ArenaInformation Object;

    public Address AvatarAddress;

    public RoundData RoundData;

    public ArenaInformationState(
        ArenaInformation arenaInformation,
        RoundData roundData,
        Address avatarAddress
    )
    {
        Object = arenaInformation;
        RoundData = roundData;
        AvatarAddress = avatarAddress;
    }

    public IValue Bencoded => Object.Serialize();
}
