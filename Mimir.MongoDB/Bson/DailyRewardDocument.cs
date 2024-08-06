using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record DailyRewardDocument(Address Address, long Object) : MimirBsonDocument(Address) { }
