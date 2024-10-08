using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Aura"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Aura : Equipment
{
    public Aura()
    {
    }

    public Aura(IValue bencoded) : base(bencoded)
    {
    }
}
