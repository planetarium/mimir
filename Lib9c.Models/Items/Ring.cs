using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Ring"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Ring : Equipment
{
    public Ring()
    {
    }

    public Ring(IValue bencoded) : base(bencoded)
    {
    }
}
