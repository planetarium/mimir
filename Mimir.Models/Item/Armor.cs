using Bencodex.Types;

namespace Mimir.Models.Item;

public class Armor(Dictionary bencoded) : Equipment(bencoded);
