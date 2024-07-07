using Bencodex.Types;

namespace Mimir.Models.Item;

public class Aura : Equipment
{
    public Aura(Dictionary bencoded)
        : base(bencoded) { }
}
