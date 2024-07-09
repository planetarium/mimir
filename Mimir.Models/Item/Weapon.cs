using Bencodex.Types;

namespace Mimir.Models.Item;

public class Weapon(Dictionary bencoded) : Equipment(bencoded);
