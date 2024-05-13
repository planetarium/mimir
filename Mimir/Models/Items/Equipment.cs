using MongoDB.Bson;

namespace Mimir.Models.Items;

public class Equipment : NonFungibleItem
{
    public int Level { get; set; }

    public Equipment(Nekoyume.Model.Item.Equipment equipment) : base(equipment)
    {
        Level = equipment.level;
    }

    public Equipment(BsonValue equipment) : base(equipment)
    {
        Level = equipment["level"].AsInt32;
    }
}
