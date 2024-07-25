using Bencodex.Types;

namespace Lib9c.Models.Item;

public class Weapon(Dictionary bencoded) : Equipment(bencoded);
