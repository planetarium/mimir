using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Weapon"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Weapon : Equipment
{
    public Weapon()
    {
    }

    public Weapon(IValue bencoded) : base(bencoded)
    {
    }
}
