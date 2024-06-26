using Libplanet.Crypto;
using Nekoyume.Model.Arena;
using Nekoyume.Model.State;
using static Nekoyume.TableData.ArenaSheet;

namespace Mimir.Worker.Models;

public class ArenaState : State
{
    public ArenaScore ArenaScoreObject;

    public ArenaInformation ArenaInformationObject;

    public Address AvatarAddress;

    public RoundData RoundData;

    public ArenaState(
        ArenaScore arenaScore,
        ArenaInformation arenaInformation,
        RoundData roundData,
        Address address,
        Address avatarAddress
    )
        : base(address)
    {
        ArenaScoreObject = arenaScore;
        ArenaInformationObject = arenaInformation;
        RoundData = roundData;
        AvatarAddress = avatarAddress;
    }
}
