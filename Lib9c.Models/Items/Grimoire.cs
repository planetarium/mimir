using Bencodex.Types;

namespace Lib9c.Models.Items;

public record Grimoire : Equipment
{
    public Grimoire()
    {
    }

    public Grimoire(IValue bencoded) : base(bencoded)
    {
    }
}
