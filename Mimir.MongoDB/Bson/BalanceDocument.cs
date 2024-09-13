using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record BalanceDocument(Address Address, string Object) : MimirBsonDocument(Address);
