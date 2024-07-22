using Mimir.Models.AdventureBoss;
using Mimir.Repositories.AdventureBoss;

namespace Mimir.GraphQL.Resolvers;

public class AdventureBossResolver
{
    public static SeasonInfo GetSeasonInfo(
        long number,
        [Service] SeasonInfoRepository seasonInfoRepository) =>
        seasonInfoRepository.GetSeasonInfo(number);
}
