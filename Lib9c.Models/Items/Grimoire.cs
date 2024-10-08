using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Grimoire"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Grimoire : Equipment
{
    public Grimoire()
    {
    }

    public Grimoire(IValue bencoded) : base(bencoded)
    {
    }
}
