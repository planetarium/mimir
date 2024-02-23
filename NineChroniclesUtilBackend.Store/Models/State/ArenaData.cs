using Nekoyume.Model.Arena;
using Nekoyume.TableData;
using Libplanet.Crypto;

namespace NineChroniclesUtilBackend.Store.Models;

public class ArenaData : BaseData
{
    public ArenaScore Score { get; }
    public ArenaInformation Information { get; }
    public ArenaSheet.RoundData RoundData { get; }
    public Address AvatarAddress { get; }

    public ArenaData(ArenaScore score, ArenaInformation information, ArenaSheet.RoundData roundData, Address avatarAddress)
    {
        Score = score;
        Information = information;
        RoundData = roundData;
        AvatarAddress = avatarAddress;
    }
}