using Lib9c.Models.States;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record RuneSlotDocument(Address Address, RuneSlotState Object)
    : IMimirBsonDocument(Address) { }
