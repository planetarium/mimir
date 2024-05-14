using MongoDB.Bson;
using Mimir.Models.Items;

namespace Mimir.Models.Avatar;

public class Inventory
{
    public List<Consumable> Consumables { get; set; }
    public List<Costume> Costumes { get; set; }
    public List<Equipment> Equipments { get; set; }
    public List<Material> Materials { get; set; }

    public Inventory(Nekoyume.Model.Item.Inventory inventory)
    {
        Consumables = inventory.Consumables
            .Select(c => new Consumable(c))
            .ToList();
        Costumes = inventory.Costumes
            .Select(c => new Costume(c))
            .ToList();
        Equipments = inventory.Equipments
            .Select(e => new Equipment(e))
            .ToList();
        Materials = inventory.Items
            .Where(i => i.item is Nekoyume.Model.Item.Material)
            .Select(i => new Material((Nekoyume.Model.Item.Material)i.item, i.count))
            .ToList();
    }

    public Inventory(BsonDocument inventory)
    {
        Consumables = inventory["Consumables"].AsBsonArray
            .Select(c => new Consumable(c.AsBsonDocument))
            .ToList();
        Costumes = inventory["Costumes"].AsBsonArray
            .Select(c => new Costume(c.AsBsonDocument))
            .ToList();
        Equipments = inventory["Equipments"].AsBsonArray
            .Select(e => new Equipment(e.AsBsonDocument))
            .ToList();
        Materials = inventory["Materials"].AsBsonArray
            .Select(m => new Material(m.AsBsonDocument))
            .ToList();
    }
}
