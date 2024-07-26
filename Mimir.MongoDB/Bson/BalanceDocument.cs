using Bencodex.Types;
using Libplanet.Types.Assets;

namespace Mimir.MongoDB.Bson;

public record BalanceDocument(FungibleAssetValue Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
