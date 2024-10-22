using Lib9c;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson;
using Nekoyume;

namespace Mimir.MongoDB
{
    public static class CollectionNames
    {
        private static readonly Currency OdinNCGCurrency = Currency.Legacy(
            "NCG",
            2,
            new Address("0x47d082a115c63e7b58b1532d20e631538eafadde"));

        private static readonly Currency HeimdallNCGCurrency = Currency.Legacy(
            "NCG",
            2,
            null);

        public static readonly Dictionary<Type, string> CollectionAndStateTypeMappings = new();
        public static readonly Dictionary<Address, string> CollectionAndAddressMappings = new();

        static CollectionNames()
        {
            RegisterCollectionAndAddressMappings();
            RegisterCollectionAndAddressMappingsForCurrencies();
            RegisterCollectionAndStateMappings();
        }

        private static Address GetAccountAddress(Currency currency) => new(currency.Hash.ToByteArray());

        /// <summary>
        /// Register MongoDB collections for various related address.
        /// </summary>
        private static void RegisterCollectionAndAddressMappings()
        {
            // Sort in alphabetical order.
            CollectionAndAddressMappings.Add(Addresses.ActionPoint, "action_point");
            CollectionAndAddressMappings.Add(Addresses.Agent, "agent");
            CollectionAndAddressMappings.Add(Addresses.Avatar, "avatar");
            CollectionAndAddressMappings.Add(Addresses.DailyReward, "daily_reward");
            CollectionAndAddressMappings.Add(Addresses.Inventory, "inventory");
            CollectionAndAddressMappings.Add(Addresses.Collection, "collection");
            // CollectionAndAddressMappings.Add(Addresses.BountyBoard, "adventure_boss_bounty_board");
            // CollectionAndAddressMappings.Add(Addresses.ExploreBoard, "adventure_boss_explore_board");
            // CollectionAndAddressMappings.Add(Addresses.ExplorerList, "adventure_boss_explorer_list");
            // CollectionAndAddressMappings.Add(Addresses. typeof(ExplorerState), "adventure_boss_explorer");
            // CollectionAndAddressMappings.Add(Addresses.AdventureBoss, "adventure_boss_season_info");
            CollectionAndAddressMappings.Add(Addresses.CombinationSlot, "all_combination_slot");

            // CollectionAndAddressMappings.Add(Addresses.QuestList, "quest_list");
            CollectionAndAddressMappings.Add(Addresses.RuneState, "all_rune");
            CollectionAndAddressMappings.Add(Addresses.WorldInformation, "world_information");
        }

        /// <summary>
        /// Register MongoDB collections for various related currency's address.
        /// </summary>
        private static void RegisterCollectionAndAddressMappingsForCurrencies()
        {
            // Sort in alphabetical order.
            CollectionAndAddressMappings.Add(GetAccountAddress(Currencies.Crystal), "balance_crystal");
            CollectionAndAddressMappings.Add(GetAccountAddress(Currencies.Garage), "balance_garage");
            CollectionAndAddressMappings.Add(GetAccountAddress(Currencies.Mead), "balance_mead");

            // FIXME: NCG currency should be mapped only once. Consider planet type of configuration.
            //        Getting NCG information in configurations might be better than trying to distinguish between Odin
            //        and Heimdall here.
            CollectionAndAddressMappings.Add(GetAccountAddress(OdinNCGCurrency), "balance_ncg");
            CollectionAndAddressMappings.Add(GetAccountAddress(HeimdallNCGCurrency), "balance_ncg");

            // Runes. Sort in alphabetical order.
            CollectionAndAddressMappings.Add(
                GetAccountAddress(Currencies.FreyaBlessingRune),
                "balance_freya_blessing_rune");
            CollectionAndAddressMappings.Add(
                GetAccountAddress(Currencies.FreyaLiberationRune),
                "balance_freya_liberation_rune");
            CollectionAndAddressMappings.Add(GetAccountAddress(Currencies.StakeRune), "balance_stake_rune");
            CollectionAndAddressMappings.Add(
                GetAccountAddress(Currencies.DailyRewardRune),
                "balance_daily_reward_rune");
            CollectionAndAddressMappings.Add(GetAccountAddress(Currencies.OdinWeaknessRune), "balance_weakness_rune");
            CollectionAndAddressMappings.Add(GetAccountAddress(Currencies.OdinWisdomRune), "balance_wisdom_rune");

            // Soul stones. Sort in alphabetical order.
            // Register here.
        }

        /// <summary>
        /// Register MongoDB collections for various related documents.
        /// </summary>
        private static void RegisterCollectionAndStateMappings()
        {
            // Sort in alphabetical order. 
            CollectionAndStateTypeMappings.Add(typeof(ActionPointDocument), "action_point");
            // CollectionAndStateTypeMappings.Add(typeof(BountyBoardDocument), "adventure_boss_bounty_board");
            // CollectionAndStateTypeMappings.Add(typeof(ExplorerDocument), "adventure_boss_explorer");
            // CollectionAndStateTypeMappings.Add(typeof(ExploreBoardDocument), "adventure_boss_explore_board");
            // CollectionAndStateTypeMappings.Add(typeof(ExplorerListDocument), "adventure_boss_explorer_list");
            // CollectionAndStateTypeMappings.Add(typeof(SeasonInfoDocument), "adventure_boss_season_info");
            CollectionAndStateTypeMappings.Add(typeof(AgentDocument), "agent");
            CollectionAndStateTypeMappings.Add(typeof(AllCombinationSlotStateDocument), "all_combination_slot");
            CollectionAndStateTypeMappings.Add(typeof(AllRuneDocument), "all_rune");
            CollectionAndStateTypeMappings.Add(typeof(AvatarDocument), "avatar");
            CollectionAndStateTypeMappings.Add(typeof(ArenaDocument), "arena");
            CollectionAndStateTypeMappings.Add(typeof(DailyRewardDocument), "daily_reward");
            CollectionAndStateTypeMappings.Add(typeof(ItemSlotDocument), "item_slot");
            CollectionAndStateTypeMappings.Add(typeof(InventoryDocument), "inventory");
            CollectionAndStateTypeMappings.Add(typeof(MetadataDocument), "metadata");
            CollectionAndStateTypeMappings.Add(typeof(PetStateDocument), "pet_state");
            CollectionAndStateTypeMappings.Add(typeof(PledgeDocument), "pledge");
            CollectionAndStateTypeMappings.Add(typeof(ProductsStateDocument), "products");
            CollectionAndStateTypeMappings.Add(typeof(ProductDocument), "product");
            // CollectionAndStateTypeMappings.Add(typeof(QuestListDocument), "quest_list");
            CollectionAndStateTypeMappings.Add(typeof(RaiderStateDocument), "raider");
            CollectionAndStateTypeMappings.Add(typeof(SheetDocument), "table_sheet");
            CollectionAndStateTypeMappings.Add(typeof(WorldBossStateDocument), "world_boss");
            CollectionAndStateTypeMappings.Add(
                typeof(WorldBossKillRewardRecordDocument),
                "world_boss_kill_reward_record");
            CollectionAndStateTypeMappings.Add(typeof(WorldInformationDocument), "world_information");
            CollectionAndStateTypeMappings.Add(typeof(StakeDocument), "stake");
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
