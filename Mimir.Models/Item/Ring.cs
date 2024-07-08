using Bencodex.Types;

namespace Mimir.Models.Item;

public class Ring : Equipment
{
    public Ring(Dictionary bencoded)
        : base(bencoded) { }
}
