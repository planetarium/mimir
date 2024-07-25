using Bencodex.Types;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Armor"/>
/// </summary>
public record Armor : Equipment
{
    public Armor(IValue bencoded) : base(bencoded)
    {
    }
}
