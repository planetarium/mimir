using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.MongoDB.Bson.AdventureBoss;

public record ExplorerDocument(Address Address, Explorer Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Bencoded;
}
