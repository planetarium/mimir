using HotChocolate.Resolvers;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Mimir.Models;
using Mimir.Repositories;
using Nekoyume;
using Nekoyume.Model.Stake;

namespace Mimir.GraphQL.Resolvers;

public class AgentResolver
{
    public static Agent GetAgent(
        IResolverContext context,
        [Service] AgentRepository agentRepo,
        [Parent] AgentObject agentObject,
        [ScopedState("agent")] Agent? agent)
    {
        if (agent is not null)
        {
            return agent;
        }

        var agentAddress = agentObject.Address;
        agent = agentRepo.GetAgent(agentAddress);
        context.ScopedContextData = context.ScopedContextData.Add("agent", agent);
        return agent;
    }

    public static int GetVersion(
        IResolverContext context,
        [Service] AgentRepository agentRepo,
        [Parent] AgentObject agentObject,
        [ScopedState("agent")] Agent? agent) =>
        GetAgent(context, agentRepo, agentObject, agent).Version;

    public static Address[] GetAvatarAddresses(
        IResolverContext context,
        [Service] AgentRepository agentRepo,
        [Parent] AgentObject agentObject,
        [ScopedState("agent")] Agent? agent) =>
        GetAgent(context, agentRepo, agentObject, agent).AvatarAddresses.Values.ToArray();

    public static int GetMonsterCollectionRound(
        IResolverContext context,
        [Service] AgentRepository agentRepo,
        [Parent] AgentObject agentObject,
        [ScopedState("agent")] Agent? agent) =>
        GetAgent(context, agentRepo, agentObject, agent).MonsterCollectionRound;

    public static AvatarObject GetAvatar(int index, [Parent] AgentObject agentObject)
    {
        var agentAddress = agentObject.Address;
        var avatarAddress = Addresses.GetAvatarAddress(agentAddress, index);
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

    public static StakeStateV2 GetStake(
        [Service] StakeRepository stakeRepo,
        [Parent] AgentObject agentObject)
    {
        var agentAddress = agentObject.Address;
        return stakeRepo.GetStakeState(agentAddress);
    }
}
