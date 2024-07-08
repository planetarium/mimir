using Bencodex.Types;

namespace Mimir.Models.Item;

public class Necklace : Equipment
{
    public Necklace(Dictionary bencoded)
        : base(bencoded) { }
}
