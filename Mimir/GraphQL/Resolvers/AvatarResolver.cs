using HotChocolate.Resolvers;
using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.GraphQL.Factories;
using Mimir.GraphQL.Objects;
using Mimir.Models;
using Mimir.Repositories;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Rune;
using Nekoyume.Model.State;

namespace Mimir.GraphQL.Resolvers;

public class AvatarResolver
{
    public static Avatar GetAvatar(
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
        context.ScopedContextData = context.ScopedContextData.Add("avatar", avatar);
        return avatar;
    }

    public static Address GetAgentAddress(
        IResolverContext context,
        [Service] AvatarRepository avatarRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("avatar")] Avatar? avatar)
    {
        if (avatarObject.AgentAddress.HasValue)
        {
            return avatarObject.AgentAddress.Value;
        }

        var agentAddr = GetAvatar(context, avatarRepo, avatarObject, planetName, avatar).AgentAddress;
        return new Address(agentAddr);
    }

    public static string GetName(
        IResolverContext context,
        [Service] AvatarRepository avatarRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("avatar")] Avatar? avatar) =>
        GetAvatar(context, avatarRepo, avatarObject, planetName, avatar).AvatarName;

    public static int GetLevel(
        IResolverContext context,
        [Service] AvatarRepository avatarRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("avatar")] Avatar? avatar) =>
        GetAvatar(context, avatarRepo, avatarObject, planetName, avatar).Level;

    public static int GetActionPoint(
        [Service] ActionPointRepository repo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName) =>
        repo.GetActionPoint(planetName, avatarObject.Address);

    public static long GetDailyRewardReceivedBlockIndex(
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
        [ScopedState("planetName")] PlanetName planetName) =>
        allRuneRepo.GetRunes(planetName, avatarObject.Address)
            .Select(RuneObjectFactory.Create)
            .ToArray();

    public static CollectionElementObject[] GetCollectionElements(
        [Service] CollectionRepository collectionRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName) =>
        collectionRepo.GetCollection(planetName, avatarObject.Address)
            .CollectionSheetIds.Select(CollectionElementObjectFactory.Create)
            .ToArray();

    public static ItemSlotState GetItemSlot(
        [Service] ItemSlotRepository itemSlotRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName,
        BattleType battleType) =>
        itemSlotRepo.GetItemSlot(planetName, avatarObject.Address, battleType);

    public static RuneSlot[] GetRuneSlots(
        [Service] RuneSlotRepository runeSlotRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("planetName")] PlanetName planetName,
        BattleType battleType) =>
        runeSlotRepo.GetRuneSlots(planetName, avatarObject.Address, battleType);
}
