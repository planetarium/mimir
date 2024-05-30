using HotChocolate.Resolvers;
using Lib9c.GraphQL.Enums;
using Lib9c.GraphQL.Types;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Mimir.Models.Agent;
using Mimir.Models.Avatar;
using Mimir.Repositories;

namespace Mimir.GraphQL.Types;

public class AvatarType : ObjectType<AvatarObject>
{
    protected override void Configure(IObjectTypeDescriptor<AvatarObject> descriptor)
    {
        descriptor
            .Field("agentAddress")
            .Type<AddressType>()
            .Resolve(context =>
            {
                var avatar = GetAvatar(context);
                if (avatar is null)
                {
                    return null;
                }

                return new Address(avatar.AgentAddress);
            });
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
    }

    private static (
        AvatarRepository avatarRepo,
        PlanetName planetName,
        Address avatarAddress)? GetSource(IResolverContext context)
    {
        var avatarRepo = context.Services.GetService<AvatarRepository>();
        if (avatarRepo is null)
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

        var avatarAddress = context.ScopedContextData.TryGetValue("avatarAddress", out var aa)
            ? (Address?)aa
            : null;
        if (avatarAddress is null)
        {
            return null;
        }

        return (avatarRepo, planetName.Value, avatarAddress.Value);
    }

    private static Avatar? GetAvatar(IResolverContext context)
    {
        var tuple = GetSource(context);
        if (tuple is null)
        {
            return null;
        }

        var (avatarRepo, planetName, avatarAddress) = tuple.Value;
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

        var (avatarRepo, planetName, avatarAddress) = tuple.Value;
        var inventory = avatarRepo.GetInventory(planetName.ToString(), avatarAddress);
        if (inventory is null)
        {
            return null;
        }

        context.ScopedContextData = context.ScopedContextData.Add("inventory", inventory);
        return inventory;
    }
}
