using Libplanet.Crypto;
using Libplanet.Types.Assets;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record BalanceDocument(Address Address, FungibleAssetValue Object) : MimirBsonDocument(Address);
