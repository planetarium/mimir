using MongoDB.Bson;
using Mimir.Models.Items;

namespace Mimir.Models.Avatar;

public class Inventory(BsonValue inventory)
{
    public List<Equipment> Equipments { get; set; } = inventory["Equipments"].AsBsonArray
        .Select(e => new Equipment(e.AsBsonDocument))
        .ToList();
}
