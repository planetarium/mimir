using System.Text.RegularExpressions;
using Libplanet.Crypto;
using Nekoyume;

namespace Mimir.Worker.Constants
{
    public static class CollectionNames
    {
        public static Dictionary<Address, string> CollectionMappings =
            new Dictionary<Address, string>();

        static CollectionNames()
        {
            MapAddressToCollectionName("Shop", Addresses.Shop);
            MapAddressToCollectionName("Ranking", Addresses.Ranking);
            MapAddressToCollectionName("WeeklyArena", Addresses.WeeklyArena);
            MapAddressToCollectionName("TableSheet", Addresses.TableSheet);
            MapAddressToCollectionName("GameConfig", Addresses.GameConfig);
            MapAddressToCollectionName("RedeemCode", Addresses.RedeemCode);
            // mongodb already have admin collection
            MapAddressToCollectionName("NineChroniclesAdmin", Addresses.Admin);
            MapAddressToCollectionName("PendingActivation", Addresses.PendingActivation);
            MapAddressToCollectionName("ActivatedAccount", Addresses.ActivatedAccount);
            MapAddressToCollectionName("Blacksmith", Addresses.Blacksmith);
            MapAddressToCollectionName("GoldCurrency", Addresses.GoldCurrency);
            MapAddressToCollectionName("GoldDistribution", Addresses.GoldDistribution);
            MapAddressToCollectionName("AuthorizedMiners", Addresses.AuthorizedMiners);
            MapAddressToCollectionName("Credits", Addresses.Credits);
            MapAddressToCollectionName("UnlockWorld", Addresses.UnlockWorld);
            MapAddressToCollectionName("UnlockEquipmentRecipe", Addresses.UnlockEquipmentRecipe);
            MapAddressToCollectionName("MaterialCost", Addresses.MaterialCost);
            MapAddressToCollectionName("StageRandomBuff", Addresses.StageRandomBuff);
            MapAddressToCollectionName("Arena", Addresses.Arena);
            MapAddressToCollectionName("SuperCraft", Addresses.SuperCraft);
            MapAddressToCollectionName("EventDungeon", Addresses.EventDungeon);
            MapAddressToCollectionName("Raid", Addresses.Raid);
            MapAddressToCollectionName("Rune", Addresses.Rune);
            MapAddressToCollectionName("Market", Addresses.Market);
            MapAddressToCollectionName("GarageWallet", Addresses.GarageWallet);
            MapAddressToCollectionName("AssetMinters", Addresses.AssetMinters);
            MapAddressToCollectionName("Agent", Addresses.Agent);
            MapAddressToCollectionName("Avatar", Addresses.Avatar);
            MapAddressToCollectionName("Inventory", Addresses.Inventory);
            MapAddressToCollectionName("WorldInformation", Addresses.WorldInformation);
            MapAddressToCollectionName("QuestList", Addresses.QuestList);
            MapAddressToCollectionName("Collection", Addresses.Collection);
            MapAddressToCollectionName("DailyReward", Addresses.DailyReward);
            MapAddressToCollectionName("ActionPoint", Addresses.ActionPoint);
            MapAddressToCollectionName("RuneState", Addresses.RuneState);
        }

        private static void MapAddressToCollectionName(string name, Address address)
        {
            // CamelCase to snake_case for MongoDB naming conventions
            string collectionName = Regex.Replace(name, @"(?<!^)([A-Z])", "_$1").ToLower();
            CollectionMappings.Add(address, collectionName);
        }
    }
}
