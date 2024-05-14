using MongoDB.Bson;

namespace Mimir.Models.Items;

public class Consumable : Item
{
    public Consumable(Nekoyume.Model.Item.Consumable consumable) :
        base(consumable, count: 1)
    {
    }

    public Consumable(BsonDocument consumable) : base(consumable)
    {
    }
}
