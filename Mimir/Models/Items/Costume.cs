using MongoDB.Bson;

namespace Mimir.Models.Items;

public class Costume : Item
{
    public Costume(Nekoyume.Model.Item.Costume costume) :
        base(costume, count: 1)
    {
    }

    public Costume(BsonDocument costume) : base(costume)
    {
    }
}
