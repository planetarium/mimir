using Bencodex.Types;

namespace Mimir.Models.Item;

public class Ring(Dictionary bencoded) : Equipment(bencoded);
