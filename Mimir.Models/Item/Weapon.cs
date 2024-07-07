using Bencodex.Types;

namespace Mimir.Models.Item;

public class Weapon : Equipment
{
    public Weapon(Dictionary bencoded)
        : base(bencoded) { }
}
