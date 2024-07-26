using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record ProductsDocument(
    Address Address,
    Nekoyume.Model.Market.ProductsState Object,
    Address AvatarAddress)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
