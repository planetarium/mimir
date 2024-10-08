using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Belt"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Belt : Equipment
{
    public Belt()
    {
    }

    public Belt(IValue bencoded) : base(bencoded)
    {
    }
}
