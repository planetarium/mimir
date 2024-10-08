using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Necklace"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Necklace : Equipment
{
    public Necklace()
    {
    }

    public Necklace(IValue bencoded) : base(bencoded)
    {
    }
}
