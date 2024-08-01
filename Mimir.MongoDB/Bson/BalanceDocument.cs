using Libplanet.Crypto;
using Libplanet.Types.Assets;

namespace Mimir.MongoDB.Bson;

public record BalanceDocument(Address Address, FungibleAssetValue Object)
    : IMimirBsonDocument(Address) { }
