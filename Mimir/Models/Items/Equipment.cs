using MongoDB.Bson;

namespace Mimir.Models.Items;

public class Equipment : Item
{
    public int Level { get; set; }

    public Equipment(Nekoyume.Model.Item.Equipment equipment) :
        base(equipment, count: 1)
    {
        Level = equipment.level;
    }

    public Equipment(BsonDocument equipment) : base(equipment)
    {
        Level = equipment["level"].AsInt32;
    }
}
