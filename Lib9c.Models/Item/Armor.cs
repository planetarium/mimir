using Bencodex.Types;

namespace Lib9c.Models.Item;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Armor"/>
/// </summary>
public record Armor : Equipment
{
    public Armor(IValue bencoded) : base(bencoded)
    {
    }
}
