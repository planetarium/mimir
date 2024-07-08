using Bencodex.Types;

namespace Mimir.Models.Item;

public class Belt : Equipment
{
    public Belt(Dictionary bencoded)
        : base(bencoded) { }
}
