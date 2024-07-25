using Bencodex.Types;

namespace Lib9c.Models.Item;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Weapon"/>
/// </summary>
public record Weapon : Equipment
{
    public Weapon(IValue bencoded) : base(bencoded)
    {
    }
}
