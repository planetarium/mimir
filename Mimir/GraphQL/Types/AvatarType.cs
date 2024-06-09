using HotChocolate.Resolvers;
using Lib9c.GraphQL.Enums;
using Lib9c.GraphQL.Types;
using Libplanet.Crypto;
using Mimir.GraphQL.Factories;
using Mimir.GraphQL.Objects;
using Mimir.Models.Agent;
using Mimir.Models.Assets;
using Mimir.Models.Avatar;
using Mimir.Repositories;

namespace Mimir.GraphQL.Types;

public class AvatarType : ObjectType<AvatarObject>
{
    protected override void Configure(IObjectTypeDescriptor<AvatarObject> descriptor)
    {
        descriptor
            .Field(f => f.Address)
            .Type<NonNullType<AddressType>>();
        descriptor
            .Field(f => f.AgentAddress)
            .Type<AddressType>()
            .Resolve(context =>
            {
                var agentAddress = context.Parent<AvatarObject>().AgentAddress;
                if (agentAddress.HasValue)
                {
                    return agentAddress;
                }

                var avatar = GetAvatar(context);
                if (avatar is null)
                {
                    return null;
                }

                agentAddress = new Address(avatar.AgentAddress);
                context.Parent<AvatarObject>().AgentAddress = agentAddress;
                return agentAddress;
            });
        descriptor
            .Field(f => f.Index)
            .Type<IntType>();
        descriptor
            .Field("name")
            .Type<StringType>()
            .Resolve(context => GetAvatar(context)?.AvatarName);
        descriptor
            .Field("level")
            .Type<IntType>()
            .Resolve(context => GetAvatar(context)?.Level);
        descriptor
            .Field("inventory")
            .Type<InventoryType>()
            .Resolve(context => GetInventory(context) is null
                ? null
                : new InventoryObject());
        descriptor
            .Field("actionPoint")
            .Type<IntType>()
            .Resolve(context => GetAvatar(context)?.ActionPoint);
        descriptor
            .Field("runes")
            .Type<NonNullType<ListType<NonNullType<RuneType>>>>()
            .Resolve(context =>
            {
                var runes = GetRunes(context);
                return runes is null
                    ? []
                    : runes.Select(RuneObjectFactory.Create).ToArray();
            });
    }

    private static (
        AvatarRepository avatarRepo,
        AllRuneRepository allRuneRepo,
        InventoryRepository inventoryRepo,
        PlanetName planetName,
        Address avatarAddress)? GetSource(IResolverContext context)
    {
        var avatarRepo = context.Services.GetService<AvatarRepository>();
        if (avatarRepo is null)
        {
            return null;
        }
        var inventoryRepo = context.Services.GetService<InventoryRepository>();
        if (inventoryRepo is null)
        {
            return null;
        }

        var allRuneRepo = context.Services.GetService<AllRuneRepository>();
        if (allRuneRepo is null)
        {
            return null;
        }

        var planetName = context.ScopedContextData.TryGetValue("planetName", out var pn)
            ? (PlanetName?)pn
            : null;
        if (planetName is null)
        {
            return null;
        }

        var avatarAddress = context.Parent<AvatarObject>().Address;
        return (avatarRepo, allRuneRepo, inventoryRepo, planetName.Value, avatarAddress);
    }

    private static Avatar? GetAvatar(IResolverContext context)
    {
        var tuple = GetSource(context);
        if (tuple is null)
        {
            return null;
        }

        var (avatarRepo, _, _, planetName, avatarAddress) = tuple.Value;
        var avatar = avatarRepo.GetAvatar(planetName.ToString(), avatarAddress);
        if (avatar is null)
        {
            return null;
        }

        context.ScopedContextData = context.ScopedContextData.Add("avatar", avatar);
        return avatar;
    }

    private static Inventory? GetInventory(IResolverContext context)
    {
        var tuple = GetSource(context);
        if (tuple is null)
        {
            return null;
        }

        var (_, _, inventoryRepo, planetName, avatarAddress) = tuple.Value;
        var inventory = inventoryRepo.GetInventory(planetName.ToString(), avatarAddress);
        if (inventory is null)
        {
            return null;
        }

        context.ScopedContextData = context.ScopedContextData.Add("inventory", inventory);
        return inventory;
    }

    private static List<Rune>? GetRunes(IResolverContext context)
    {
        var tuple = GetSource(context);
        if (tuple is null)
        {
            return null;
        }

        var (_, allRuneRepo, _, planetName, avatarAddress) = tuple.Value;
        var runes = allRuneRepo.GetRunes(planetName, avatarAddress);
        if (runes is null)
        {
            return null;
        }

        context.ScopedContextData = context.ScopedContextData.Add("runes", runes);
        return runes;
    }
}
