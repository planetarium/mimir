using Lib9c;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Handler;

namespace Mimir.Worker.Constants
{
    public static class CollectionNames
    {
        public static readonly Dictionary<Type, string> CollectionAndStateTypeMappings = new();
        public static readonly Dictionary<Address, string> CollectionAndAddressMappings = new();

        static CollectionNames()
        {
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.Agent, "agent");
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.Avatar, "avatar");
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.ActionPoint, "action_point");
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.DailyReward, "daily_reward");
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.Inventory, "inventory");
            // CollectionAndAddressMappings.Add(Nekoyume.Addresses.WorldInformation, "world_information");
            // CollectionAndAddressMappings.Add(Nekoyume.Addresses.QuestList, "quest_list");
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.RuneState, "all_rune");
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.Collection, "collection");
            // CollectionAndAddressMappings.Add(Nekoyume.Addresses.BountyBoard, "adventure_boss_bounty_board");
            // CollectionAndAddressMappings.Add(Nekoyume.Addresses.ExploreBoard, "adventure_boss_explore_board");
            // CollectionAndAddressMappings.Add(Nekoyume.Addresses.ExplorerList, "adventure_boss_explorer_list");
            // CollectionAndAddressMappings.Add(Nekoyume.Addresses. typeof(ExplorerState), "adventure_boss_explorer");
            // CollectionAndAddressMappings.Add(Nekoyume.Addresses.AdventureBoss, "adventure_boss_season_info");

            CollectionAndAddressMappings.Add(
                new Address(AddressHandlerMappings.OdinNCGCurrency.Hash.ToByteArray()),
                "balance_ncg"
            );
            CollectionAndAddressMappings.Add(
                new Address(Currencies.Crystal.Hash.ToByteArray()),
                "balance_crystal"
            );
            CollectionAndAddressMappings.Add(
                new Address(Currencies.StakeRune.Hash.ToByteArray()),
                "balance_stake_rune"
            );
            CollectionAndAddressMappings.Add(
                new Address(Currencies.DailyRewardRune.Hash.ToByteArray()),
                "balance_daily_reward_rune"
            );
            CollectionAndAddressMappings.Add(
                new Address(Currencies.Garage.Hash.ToByteArray()),
                "balance_garage"
            );
            CollectionAndAddressMappings.Add(
                new Address(Currencies.Mead.Hash.ToByteArray()),
                "balance_mead"
            );
            CollectionAndAddressMappings.Add(
                new Address(Currencies.FreyaBlessingRune.Hash.ToByteArray()),
                "balance_freya_blessing_rune"
            );
            CollectionAndAddressMappings.Add(
                new Address(Currencies.FreyaLiberationRune.Hash.ToByteArray()),
                "balance_freya_liberation_rune"
            );
            CollectionAndAddressMappings.Add(
                new Address(Currencies.OdinWeaknessRune.Hash.ToByteArray()),
                "balance_weakness_rune"
            );
            CollectionAndAddressMappings.Add(
                new Address(Currencies.OdinWisdomRune.Hash.ToByteArray()),
                "balance_wisdom_rune"
            );
            CollectionAndStateTypeMappings.Add(typeof(SheetDocument), "table_sheet");
            CollectionAndStateTypeMappings.Add(typeof(AgentDocument), "agent");
            CollectionAndStateTypeMappings.Add(typeof(AvatarDocument), "avatar");
            CollectionAndStateTypeMappings.Add(typeof(ActionPointDocument), "action_point");
            CollectionAndStateTypeMappings.Add(typeof(DailyRewardDocument), "daily_reward");
            CollectionAndStateTypeMappings.Add(typeof(InventoryDocument), "inventory");
            CollectionAndStateTypeMappings.Add(typeof(ArenaDocument), "arena");
            CollectionAndStateTypeMappings.Add(typeof(ProductsStateDocument), "products");
            CollectionAndStateTypeMappings.Add(typeof(ProductDocument), "product");
            // CollectionAndStateTypeMappings.Add(typeof(QuestListDocument), "quest_list");
            // CollectionAndStateTypeMappings.Add(typeof(WorldInformationDocument), "world_information");
            // CollectionAndStateTypeMappings.Add(typeof(ItemSlotDocument), "item_slot");
            CollectionAndStateTypeMappings.Add(typeof(RuneSlotDocument), "rune_slot");
            CollectionAndStateTypeMappings.Add(typeof(WorldBossStateDocument), "world_boss");
            CollectionAndStateTypeMappings.Add(
                typeof(WorldBossKillRewardRecordDocument),
                "world_boss_kill_reward_record"
            );
            CollectionAndStateTypeMappings.Add(typeof(RaiderStateDocument), "raider");
            // CollectionAndStateTypeMappings.Add(typeof(StakeDocument), "stake");
            // CollectionAndStateTypeMappings.Add(typeof(CombinationSlotDocument), "combination_slot");
            // CollectionAndStateTypeMappings.Add(typeof(PetDocument), "pet_state");
            // CollectionAndStateTypeMappings.Add(typeof(BountyBoardDocument), "adventure_boss_bounty_board");
            // CollectionAndStateTypeMappings.Add(typeof(ExploreBoardDocument), "adventure_boss_explore_board");
            // CollectionAndStateTypeMappings.Add(typeof(ExplorerListDocument), "adventure_boss_explorer_list");
            // CollectionAndStateTypeMappings.Add(typeof(ExplorerDocument), "adventure_boss_explorer");
            // CollectionAndStateTypeMappings.Add(typeof(SeasonInfoDocument), "adventure_boss_season_info");
        }

        public static string GetCollectionName<T>()
        {
            if (!CollectionAndStateTypeMappings.TryGetValue(typeof(T), out var collectionName))
            {
                throw new InvalidOperationException(
                    $"No collection mapping found for state type: {typeof(T).FullName}"
                );
            }

            return collectionName;
        }

        public static string GetCollectionName(Type type)
        {
            if (!CollectionAndStateTypeMappings.TryGetValue(type, out var collectionName))
            {
                throw new InvalidOperationException(
                    $"No collection mapping found for state type: {type.FullName}"
                );
            }

            return collectionName;
        }

        public static string GetCollectionName(Address accountAddress)
        {
            if (!CollectionAndAddressMappings.TryGetValue(accountAddress, out var collectionName))
            {
                throw new InvalidOperationException(
                    $"No collection mapping found for state type: {accountAddress}"
                );
            }

            return collectionName;
        }
    }
}
