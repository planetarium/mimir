using MongoDB.Bson;
using Mimir.Models.Items;
using Nekoyume.Model.Item;

namespace Mimir.Models.Avatar;

public class Inventory
{
    public List<Item> Consumables { get; set; }
    public List<Item> Costumes { get; set; }
    public List<Item> Equipments { get; set; }
    public List<Item> Materials { get; set; }

    public Inventory(Nekoyume.Model.Item.Inventory inventory)
    {
        Consumables = new List<Item>();
        Costumes = new List<Item>();
        Equipments = new List<Item>();
        Materials = new List<Item>();
        var inventoryItems = inventory.Items;
        var inventoryItemsCount = inventoryItems.Count;
        for (var i = 0; i < inventoryItemsCount; i++)
        {
            var inventoryItem = inventoryItems[i];
            switch (inventoryItem.item.ItemType)
            {
                case ItemType.Consumable:
                    Consumables.Add(new Item(inventoryItem));
                    break;
                case ItemType.Costume:
                    Costumes.Add(new Item(inventoryItem));
                    break;
                case ItemType.Equipment:
                    Equipments.Add(new Item(inventoryItem));
                    break;
                case ItemType.Material:
                    Materials.Add(new Item(inventoryItem));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(inventoryItem.item.ItemType),
                        $"Invalid ItemType: {inventoryItem.item.ItemType}");
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
            var item = new Item(inventoryItem.AsBsonDocument);
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
                        $"Invalid ItemType: {item.ItemType}");
            }
        }
    }
}
