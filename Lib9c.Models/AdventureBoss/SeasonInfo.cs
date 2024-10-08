using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.AdventureBoss;

[BsonIgnoreExtraElements]
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
