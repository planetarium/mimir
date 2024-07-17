using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.Arena;
using static Nekoyume.TableData.ArenaSheet;

namespace Mimir.Worker.Models;

public class ArenaScoreState : IBencodable
{
    public ArenaScore Object;

    public Address AvatarAddress;

    public RoundData RoundData;

    public ArenaScoreState(ArenaScore arenaScore, RoundData roundData, Address avatarAddress)
    {
        Object = arenaScore;
        RoundData = roundData;
        AvatarAddress = avatarAddress;
    }

    public IValue Bencoded => Object.Serialize();
}
