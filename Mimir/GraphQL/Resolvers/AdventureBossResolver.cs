using HotChocolate.Resolvers;
using Mimir.Models.AdventureBoss;
using Mimir.Repositories.AdventureBoss;

namespace Mimir.GraphQL.Resolvers;

public class AdventureBossResolver
{
    public static SeasonInfo GetSeasonInfoAsync(
        IResolverContext context,
        [Service] SeasonInfoRepository seasonInfoRepository,
        [ScopedState("seasonInfo")] SeasonInfo? seasonInfo)
    {
        if (seasonInfo is not null)
        {
            return seasonInfo;
        }

        seasonInfo = seasonInfoRepository.GetSeasonInfo();
        context.ScopedContextData = context.ScopedContextData.Add("seasonInfo", seasonInfo);
        return seasonInfo;
    }
}
