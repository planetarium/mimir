using HotChocolate.Resolvers;
using Lib9c.GraphQL.Enums;
using Mimir.GraphQL.Objects;
using Mimir.Repositories;

namespace Mimir.GraphQL.Resolvers;

public class DailyRewardReceivedBlockIndexResolver
{
    public static long? Resolve(
        IResolverContext context,
        [Service] DailyRewardRepository dailyRewardRepository)
    {
        if (!context.ScopedContextData.TryGetValue("planetName", out var pn) ||
            pn is not PlanetName planetName)
        {
            return null;
        }

        var avatarAddress = context.Parent<AvatarObject>().Address;
        return dailyRewardRepository.GetDailyReward(planetName, avatarAddress);
    }
}
