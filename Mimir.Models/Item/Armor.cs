using Bencodex.Types;

namespace Mimir.Models.Item;

public class Armor : Equipment
{
    public Armor(Dictionary bencoded)
        : base(bencoded) { }
}
