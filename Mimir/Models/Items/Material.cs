using MongoDB.Bson;

namespace Mimir.Models.Items;

public class Material : Item
{
    public Material(Nekoyume.Model.Item.Material material, int count) :
        base(material, count)
    {
    }

    public Material(BsonDocument material) : base(material)
    {
    }
}
