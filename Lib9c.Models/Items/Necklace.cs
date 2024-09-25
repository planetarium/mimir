using Bencodex.Types;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Necklace"/>
/// </summary>
public record Necklace : Equipment
{
    public Necklace()
    {
    }

    public Necklace(IValue bencoded) : base(bencoded)
    {
    }
}
