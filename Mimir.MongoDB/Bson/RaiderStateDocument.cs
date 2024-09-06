using Lib9c.Models.States;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record RaiderStateDocument(Address Address, RaiderState Object) : MimirBsonDocument(Address) { }
