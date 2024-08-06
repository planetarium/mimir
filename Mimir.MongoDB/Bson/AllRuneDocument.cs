using Lib9c.Models.States;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record AllRuneDocument(Address Address, AllRuneState Object)
    : MimirBsonDocument(Address) { }
