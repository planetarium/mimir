using MongoDB.Bson;
using NineChroniclesUtilBackend.Models.Items;

namespace NineChroniclesUtilBackend.Models.Avatar;

public class Inventory(BsonValue inventory)
{
    public List<Equipment> Equipments { get; set; } = inventory["Equipments"].AsBsonArray
        .Select(e => new Equipment(e.AsBsonDocument))
        .ToList();
}
