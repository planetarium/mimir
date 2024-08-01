using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record ActionPointDocument(Address Address, int Object) : IMimirBsonDocument(Address) { }
