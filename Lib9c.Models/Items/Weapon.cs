using Bencodex.Types;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Weapon"/>
/// </summary>
public record Weapon : Equipment
{
    public Weapon()
    {
    }

    public Weapon(IValue bencoded) : base(bencoded)
    {
    }
}
