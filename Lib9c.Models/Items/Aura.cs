using Bencodex.Types;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Aura"/>
/// </summary>
public record Aura : Equipment
{
    public Aura(IValue bencoded) : base(bencoded)
    {
    }
}
