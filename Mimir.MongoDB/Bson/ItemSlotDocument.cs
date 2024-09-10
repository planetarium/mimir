using Lib9c.Models.States;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record ItemSlotDocument(Address Address, ItemSlotState Object)
    : MimirBsonDocument(Address) { }
