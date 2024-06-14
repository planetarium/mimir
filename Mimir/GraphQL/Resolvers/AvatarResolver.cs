using HotChocolate.Resolvers;
using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.GraphQL.Factories;
using Mimir.GraphQL.Objects;
using Mimir.Models;
using Mimir.Repositories;

namespace Mimir.GraphQL.Resolvers;

public class AvatarResolver
{
    public static Avatar? GetAvatar(
        IResolverContext context,
        [Service] AvatarRepository avatarRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("avatar")] Avatar? avatar)
    {
        if (avatar is not null)
        {
            return avatar;
        }

        var avatarAddress = avatarObject.Address;
        avatar = avatarRepo.GetAvatar(planetName, avatarAddress);
        if (avatar is null)
        {
            return null;
        }

        context.ScopedContextData = context.ScopedContextData.Add("avatar", avatar);
        return avatar;
    }

    public static Address? GetAgentAddress(
        IResolverContext context,
        [Service] AvatarRepository avatarRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("avatar")] Avatar? avatar)
    {
        if (avatarObject.AgentAddress is not null)
        {
            return avatarObject.AgentAddress;
        }

        var agentAddr = GetAvatar(context, avatarRepo, avatarObject, planetName, avatar)?.AgentAddress;
        if (agentAddr is null)
        {
            return null;
        }

        return new Address(agentAddr);
    }

    public static string? GetName(
        IResolverContext context,
        [Service] AvatarRepository avatarRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("avatar")] Avatar? avatar) =>
        GetAvatar(context, avatarRepo, avatarObject, planetName, avatar)?.AvatarName;

    public static int? GetLevel(
        IResolverContext context,
        [Service] AvatarRepository avatarRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("avatar")] Avatar? avatar) =>
        GetAvatar(context, avatarRepo, avatarObject, planetName, avatar)?.Level;

    public static int? GetActionPoint(
        [Service] ActionPointRepository repo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName) =>
        repo.GetActionPoint(planetName, avatarObject.Address);

    public static long? GetDailyRewardReceivedBlockIndex(
        [Service] DailyRewardRepository repo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName) =>
        repo.GetDailyReward(planetName, avatarObject.Address);

    public static InventoryObject GetInventory(
        [Parent] AvatarObject avatarObject) =>
        new(avatarObject.Address);

    public static RuneObject[] GetRunes(
        [Service] AllRuneRepository allRuneRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName)
    {
        var avatarAddress = avatarObject.Address;
        var runes = allRuneRepo.GetRunes(planetName, avatarAddress);
        return runes is null
            ? []
            : runes.Select(RuneObjectFactory.Create).ToArray();
    }

    public static CollectionElementObject[] GetCollectionElements(
        [Service] CollectionRepository collectionRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName)
    {
        var avatarAddress = avatarObject.Address;
        var collection = collectionRepo.GetCollection(planetName, avatarAddress);
        return collection is null
            ? []
            : collection.CollectionSheetIds.Select(CollectionElementObjectFactory.Create).ToArray();
    }
}
