using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.MongoDB.Bson.AdventureBoss;

public record ExplorerListDocument(Address Address, ExplorerList Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Bencoded;
}
