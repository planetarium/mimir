using Bencodex.Types;

namespace Lib9c.Models.Item;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Belt"/>
/// </summary>
public record Belt : Equipment
{
    public Belt(IValue bencoded) : base(bencoded)
    {
    }
}
