using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[BsonIgnoreExtraElements]
public record DailyRewardDocument(Address Address, long Object) : MimirBsonDocument(Address);
