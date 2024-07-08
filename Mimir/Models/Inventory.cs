using MongoDB.Bson;
using Nekoyume.Model.Item;

namespace Mimir.Models;

public class Inventory
{
    public List<Assets.Item> Consumables { get; set; }
    public List<Assets.Item> Costumes { get; set; }
    public List<Assets.Item> Equipments { get; set; }
    public List<Assets.Item> Materials { get; set; }

    public Inventory(Nekoyume.Model.Item.Inventory inventory)
    {
        Consumables = new List<Assets.Item>();
        Costumes = new List<Assets.Item>();
        Equipments = new List<Assets.Item>();
        Materials = new List<Assets.Item>();
        var inventoryItems = inventory.Items;
        var inventoryItemsCount = inventoryItems.Count;
        for (var i = 0; i < inventoryItemsCount; i++)
        {
            var inventoryItem = inventory.Items[i];
            switch (inventoryItem.item.ItemType)
            {
                case ItemType.Consumable:
                    Consumables.Add(new Assets.Item(inventoryItem));
                    break;
                case ItemType.Costume:
                    Costumes.Add(new Assets.Item(inventoryItem));
                    break;
                case ItemType.Equipment:
                    Equipments.Add(new Assets.Item(inventoryItem));
                    break;
                case ItemType.Material:
                    Materials.Add(new Assets.Item(inventoryItem));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(inventoryItem.item.ItemType),
                        $"Invalid Assets.ItemType: {inventoryItem.item.ItemType}");
            }
        }
    }

    public Inventory(BsonDocument inventory)
    {
        Consumables = [];
        Costumes = [];
        Equipments = [];
        Materials = [];

        if (!inventory.Contains("Items"))
        {
            return;
        }

        var inventoryItems = inventory["Items"].AsBsonArray;
        foreach (var inventoryItem in inventoryItems)
        {
            var item = new Assets.Item(inventoryItem.AsBsonDocument);
            switch (item.ItemType)
            {
                case ItemType.Consumable:
                    Consumables.Add(item);
                    break;
                case ItemType.Costume:
                    Costumes.Add(item);
                    break;
                case ItemType.Equipment:
                    Equipments.Add(item);
                    break;
                case ItemType.Material:
                    Materials.Add(item);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(item.ItemType),
                        $"Invalid Assets.ItemType: {item.ItemType}");
            }
        }
    }
}
