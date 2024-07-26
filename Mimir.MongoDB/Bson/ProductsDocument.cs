using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record ProductsDocument(
    Nekoyume.Model.Market.ProductsState Object,
    Address AvatarAddress)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
