using MongoDB.Bson;
using Mimir.Models.Items;

namespace Mimir.Models.Avatar;

public class Inventory
{
    public List<Equipment> Equipments { get; set; }

    public Inventory(Nekoyume.Model.Item.Inventory inventory)
    {
        Equipments = inventory.Equipments
            .Select(e => new Equipment(e))
            .ToList();
    }
    
    public Inventory(BsonValue inventory)
    {
        Equipments = inventory["Equipments"].AsBsonArray
            .Select(e => new Equipment(e.AsBsonDocument))
            .ToList();
    }
}
