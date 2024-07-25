using Bencodex.Types;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Belt"/>
/// </summary>
public record Belt : Equipment
{
    public Belt(IValue bencoded) : base(bencoded)
    {
    }
}
