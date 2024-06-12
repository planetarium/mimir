using HotChocolate.Resolvers;
using Lib9c.GraphQL.Enums;
using Lib9c.GraphQL.Types;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Mimir.Models;
using Mimir.Repositories;
using Nekoyume;

namespace Mimir.GraphQL.Types;

public class AgentType : ObjectType<AgentObject>
{
    protected override void Configure(IObjectTypeDescriptor<AgentObject> descriptor)
    {
        descriptor
            .Field("version")
            .Type<IntType>()
            .Resolve(context => GetAgent(context)?.Version ?? null);
        descriptor
            .Field(f => f.Address)
            .Type<NonNullType<AddressType>>();
        descriptor
            .Field("avatarAddresses")
            .Type<NonNullType<ListType<NonNullType<AddressType>>>>()
            .Resolve(context => GetAgent(context)?.AvatarAddresses.Values.ToArray() ?? []);
        descriptor
            .Field("monsterCollectionRound")
            .Type<IntType>()
            .Resolve(context => GetAgent(context)?.MonsterCollectionRound ?? null);
        descriptor
            .Field("avatar")
            .Argument("index", a => a.Type<NonNullType<IntType>>())
            .Type<AvatarType>()
            .Resolve(context =>
            {
                var agentAddress = context.Parent<AgentObject>().Address;
                var index = context.ArgumentValue<int>("index");
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
            });
        descriptor
            .Field("avatars")
            .Type<NonNullType<ListType<NonNullType<AvatarType>>>>()
            .Resolve(context =>
            {
                var agentAddress = context.Parent<AgentObject>().Address;
                return Enumerable.Range(0, GameConfig.SlotCount)
                    .Select(i => (index: i, avatarAddress: Addresses.GetAvatarAddress(agentAddress, i)))
                    .Select(tuple => new AvatarObject(
                        tuple.avatarAddress,
                        agentAddress,
                        tuple.index))
                    .ToList();
            });
    }

    private static (
        AgentRepository agentRepo,
        PlanetName planetName,
        Address agentAddress)? GetSource(IResolverContext context)
    {
        var agentRepo = context.Services.GetService<AgentRepository>();
        if (agentRepo is null)
        {
            return null;
        }

        if (!context.ScopedContextData.TryGetValue("planetName", out var pn) ||
            pn is not PlanetName planetName)
        {
            return null;
        }

        var agentAddress = context.Parent<AgentObject>().Address;
        return (agentRepo, planetName, agentAddress);
    }

    private static Agent? GetAgent(IResolverContext context)
    {
        var agent = context.ScopedContextData.TryGetValue("agent", out var a)
            ? (Agent?)a
            : null;
        if (agent is not null)
        {
            return agent;
        }

        var tuple = GetSource(context);
        if (tuple is null)
        {
            return null;
        }

        var (agentRepo, planetName, agentAddress) = tuple.Value;
        agent = agentRepo.GetAgent(planetName, agentAddress);
        if (agent is null)
        {
            return null;
        }

        context.ScopedContextData = context.ScopedContextData.Add("agent", agent);
        return agent;
    }
}
