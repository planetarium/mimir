using Lib9c;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Bson.AdventureBoss;
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
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.Inventory, "inventory");
            CollectionAndAddressMappings.Add(
                Nekoyume.Addresses.WorldInformation,
                "world_information"
            );
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.ActionPoint, "action_point");
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.QuestList, "quest_list");
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.RuneState, "all_rune");
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.Collection, "collection");
            CollectionAndAddressMappings.Add(Nekoyume.Addresses.DailyReward, "daily_reward");
            // CollectionAndAddressMappings.Add(Nekoyume.Addresses.BountyBoard, "adventure_boss_bounty_board");
            // CollectionAndAddressMappings.Add(Nekoyume.Addresses.ExploreBoard, "adventure_boss_explore_board");
            // CollectionAndAddressMappings.Add(Nekoyume.Addresses.ExplorerList, "adventure_boss_explorer_list");
            // CollectionAndAddressMappings.Add(Nekoyume.Addresses. typeof(ExplorerState), "adventure_boss_explorer");
            // CollectionAndAddressMappings.Add(Nekoyume.Addresses.AdventureBoss, "adventure_boss_season_info");
            RegisterBalanceAddress(AddressHandlerMappings.OdinNCGCurrency);
            RegisterBalanceAddress(Currencies.Crystal);
            RegisterBalanceAddress(Currencies.StakeRune);
            RegisterBalanceAddress(Currencies.DailyRewardRune);
            RegisterBalanceAddress(Currencies.Garage);
            RegisterBalanceAddress(Currencies.Mead);
            RegisterBalanceAddress(Currencies.FreyaBlessingRune);
            RegisterBalanceAddress(Currencies.FreyaLiberationRune);
            RegisterBalanceAddress(Currencies.OdinWeaknessRune);
            RegisterBalanceAddress(Currencies.OdinWisdomRune);

            CollectionAndStateTypeMappings.Add(typeof(AgentState), "agent");
            CollectionAndStateTypeMappings.Add(typeof(Lib9c.Models.States.AvatarState), "avatar");
            CollectionAndStateTypeMappings.Add(typeof(InventoryState), "inventory");
            CollectionAndStateTypeMappings.Add(typeof(QuestListState), "quest_list");
            CollectionAndStateTypeMappings.Add(typeof(WorldInformationState), "world_information");
            CollectionAndStateTypeMappings.Add(typeof(ActionPointState), "action_point");
            CollectionAndStateTypeMappings.Add(typeof(SheetState), "table_sheet");
            CollectionAndStateTypeMappings.Add(typeof(ArenaScoreState), "arena_score");
            CollectionAndStateTypeMappings.Add(typeof(ArenaInformationState), "arena_information");
            CollectionAndStateTypeMappings.Add(typeof(AllRuneState), "all_rune");
            CollectionAndStateTypeMappings.Add(typeof(CollectionState), "collection");
            CollectionAndStateTypeMappings.Add(typeof(DailyRewardState), "daily_reward");
            CollectionAndStateTypeMappings.Add(typeof(ProductsState), "products");
            CollectionAndStateTypeMappings.Add(typeof(ProductState), "product");
            CollectionAndStateTypeMappings.Add(typeof(ItemSlotState), "item_slot");
            CollectionAndStateTypeMappings.Add(typeof(Lib9c.Models.States.RuneSlotState), "rune_slot");
            CollectionAndStateTypeMappings.Add(typeof(WorldBossState), "world_boss");
            CollectionAndStateTypeMappings.Add(
                typeof(WorldBossKillRewardRecordState),
                "world_boss_kill_reward_record"
            );
            CollectionAndStateTypeMappings.Add(typeof(RaiderState), "raider");
            CollectionAndStateTypeMappings.Add(typeof(StakeState), "stake");
            CollectionAndStateTypeMappings.Add(typeof(CombinationSlotState), "combination_slot");
            CollectionAndStateTypeMappings.Add(typeof(PetState), "pet_state");
            CollectionAndStateTypeMappings.Add(typeof(BountyBoardState), "adventure_boss_bounty_board");
            CollectionAndStateTypeMappings.Add(typeof(ExploreBoardState), "adventure_boss_explore_board");
            CollectionAndStateTypeMappings.Add(typeof(ExplorerListState), "adventure_boss_explorer_list");
            CollectionAndStateTypeMappings.Add(typeof(ExplorerState), "adventure_boss_explorer");
            CollectionAndStateTypeMappings.Add(typeof(SeasonInfoState), "adventure_boss_season_info");

            // The `Raw` fields of the documents' in 'balances' collection,
            // will not have the original state.  In Libplanet implementation,
            // each currency has an account trie. And their states' raw value
            // may be `Bencodex.Types.Integer`-typed. But `BalanceState` stores
            // serialized `FungibleAssetValue` instead of `Integer` to query
            // easily without fetching `Currency` from other source.
            CollectionAndStateTypeMappings.Add(typeof(BalanceState), "balances");
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

        private static void RegisterBalanceAddress(Currency currency)
        {
            CollectionAndAddressMappings.Add(new Address(currency.Hash.ToByteArray()), "balances");
        }
    }
}
