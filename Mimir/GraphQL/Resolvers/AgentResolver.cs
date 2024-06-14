using HotChocolate.Resolvers;
using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Mimir.Models;
using Mimir.Repositories;
using Nekoyume;

namespace Mimir.GraphQL.Resolvers;

public class AgentResolver
{
    public static Agent? GetAgent(
        IResolverContext context,
        [Service] AgentRepository agentRepo,
        [Parent] AgentObject agentObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("agent")] Agent? agent)
    {
        if (agent is not null)
        {
            return agent;
        }

        var agentAddress = agentObject.Address;
        agent = agentRepo.GetAgent(planetName, agentAddress);
        if (agent is null)
        {
            return null;
        }

        context.ScopedContextData = context.ScopedContextData.Add("agent", agent);
        return agent;
    }

    public static int? GetVersion(
        IResolverContext context,
        [Service] AgentRepository agentRepo,
        [Parent] AgentObject agentObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("agent")] Agent? agent) =>
        GetAgent(context, agentRepo, agentObject, planetName, agent)?.Version;

    public static Address[] GetAvatarAddresses(
        IResolverContext context,
        [Service] AgentRepository agentRepo,
        [Parent] AgentObject agentObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("agent")] Agent? agent) =>
        GetAgent(context, agentRepo, agentObject, planetName, agent)?.AvatarAddresses.Values.ToArray() ?? [];

    public static int? GetMonsterCollectionRound(
        IResolverContext context,
        [Service] AgentRepository agentRepo,
        [Parent] AgentObject agentObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("agent")] Agent? agent) =>
        GetAgent(context, agentRepo, agentObject, planetName, agent)?.MonsterCollectionRound;

    public static AvatarObject? GetAvatar(int index, [Parent] AgentObject agentObject)
    {
        var agentAddress = agentObject.Address;
        Address avatarAddress;
        try
        {
            avatarAddress = Addresses.GetAvatarAddress(agentAddress, index);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }

        return new AvatarObject(avatarAddress, agentAddress, index);
    }

    public static List<AvatarObject> GetAvatars([Parent] AgentObject agentObject)
    {
        var agentAddress = agentObject.Address;
        return Enumerable.Range(0, GameConfig.SlotCount)
            .Select(i => (index: i, avatarAddress: Addresses.GetAvatarAddress(agentAddress, i)))
            .Select(tuple => new AvatarObject(
                tuple.avatarAddress,
                agentAddress,
                tuple.index))
            .ToList();
    }
}
