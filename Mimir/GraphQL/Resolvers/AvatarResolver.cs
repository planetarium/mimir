using HotChocolate.Resolvers;
using Lib9c.Models.Runes;
using Libplanet.Crypto;
using Mimir.GraphQL.Factories;
using Mimir.GraphQL.Objects;
using Mimir.Models;
using Mimir.Repositories;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.State;

namespace Mimir.GraphQL.Resolvers;

public class AvatarResolver
{
    public static Avatar GetAvatar(
        IResolverContext context,
        [Service] AvatarRepository avatarRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("avatar")] Avatar? avatar)
    {
        if (avatar is not null)
        {
            return avatar;
        }

        var avatarAddress = avatarObject.Address;
        avatar = avatarRepo.GetAvatar(avatarAddress);
        context.ScopedContextData = context.ScopedContextData.Add("avatar", avatar);
        return avatar;
    }

    public static Address GetAgentAddress(
        IResolverContext context,
        [Service] AvatarRepository avatarRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("avatar")] Avatar? avatar)
    {
        if (avatarObject.AgentAddress.HasValue)
        {
            return avatarObject.AgentAddress.Value;
        }

        var agentAddr = GetAvatar(context, avatarRepo, avatarObject, avatar).AgentAddress;
        return new Address(agentAddr);
    }

    public static string GetName(
        IResolverContext context,
        [Service] AvatarRepository avatarRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("avatar")] Avatar? avatar) =>
        GetAvatar(context, avatarRepo, avatarObject, avatar).AvatarName;

    public static int GetLevel(
        IResolverContext context,
        [Service] AvatarRepository avatarRepo,
        [Parent] AvatarObject avatarObject,
        [ScopedState("avatar")] Avatar? avatar) =>
        GetAvatar(context, avatarRepo, avatarObject, avatar).Level;

    public static int GetActionPoint(
        [Service] ActionPointRepository repo,
        [Parent] AvatarObject avatarObject) =>
        repo.GetActionPoint(avatarObject.Address);

    public static long GetDailyRewardReceivedBlockIndex(
        [Service] DailyRewardRepository repo,
        [Parent] AvatarObject avatarObject) =>
        repo.GetDailyReward(avatarObject.Address);

    public static InventoryObject GetInventory(
        [Parent] AvatarObject avatarObject) =>
        new(avatarObject.Address);

    public static RuneObject[] GetRunes(
        [Service] AllRuneRepository allRuneRepo,
        [Parent] AvatarObject avatarObject) =>
        allRuneRepo.GetRunes(avatarObject.Address)
            .Select(RuneObjectFactory.Create)
            .ToArray();

    public static CollectionElementObject[] GetCollectionElements(
        [Service] CollectionRepository collectionRepo,
        [Parent] AvatarObject avatarObject) =>
        collectionRepo.GetCollection(avatarObject.Address)
            .CollectionSheetIds.Select(CollectionElementObjectFactory.Create)
            .ToArray();

    public static ItemSlotState GetItemSlot(
        [Service] ItemSlotRepository itemSlotRepo,
        [Parent] AvatarObject avatarObject,
        BattleType battleType) =>
        itemSlotRepo.GetItemSlot(avatarObject.Address, battleType);

    public static IEnumerable<RuneSlot> GetRuneSlots(
        [Service] RuneSlotRepository runeSlotRepo,
        [Parent] AvatarObject avatarObject,
        BattleType battleType) =>
        runeSlotRepo.GetRuneSlotState(avatarObject.Address, battleType).Slots;
}
