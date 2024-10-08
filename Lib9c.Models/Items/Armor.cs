using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Armor"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Armor : Equipment
{
    public Armor()
    {
    }

    public Armor(IValue bencoded) : base(bencoded)
    {
    }
}
