using Bencodex.Types;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Ring"/>
/// </summary>
public record Ring : Equipment
{
    public Ring()
    {
    }

    public Ring(IValue bencoded) : base(bencoded)
    {
    }
}
