using Libplanet.Crypto;

namespace Lib9c.Models.AdventureBoss;

public record SeasonInfo(
    Address Address,
    long Season,
    long StartBlockIndex,
    long EndBlockIndex,
    long NextStartBlockIndex,
    int BossId)
{
    public SeasonInfo(Address address, Nekoyume.Model.AdventureBoss.SeasonInfo seasonInfo) :
        this(
            address,
            seasonInfo.Season,
            seasonInfo.StartBlockIndex,
            seasonInfo.EndBlockIndex,
            seasonInfo.NextStartBlockIndex,
            seasonInfo.BossId)
    {
    }
}
