using MongoDB.Bson;

namespace Mimir.Models.Items;

public class Equipment : Item
{
    public Guid NonFungibleId { get; set; }
    public int Level { get; set; }

    public Equipment(Nekoyume.Model.Item.Equipment equipment) :
        base(equipment, count: 1)
    {
        NonFungibleId = equipment.NonFungibleId;
        Level = equipment.level;
    }

    public Equipment(BsonDocument equipment) : base(equipment)
    {
        NonFungibleId = Guid.Parse(equipment["NonFungibleId"].AsString);
        Level = equipment["level"].AsInt32;
    }
}
