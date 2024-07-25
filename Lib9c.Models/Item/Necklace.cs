using Bencodex.Types;

namespace Lib9c.Models.Item;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Necklace"/>
/// </summary>
public record Necklace : Equipment
{
    public Necklace(IValue bencoded) : base(bencoded)
    {
    }
}
