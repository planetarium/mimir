using Bencodex.Types;

namespace Mimir.Models.Item;

public class Necklace(Dictionary bencoded) : Equipment(bencoded);
