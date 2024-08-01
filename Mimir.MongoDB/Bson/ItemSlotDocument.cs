using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record ItemSlotDocument(Address Address, Nekoyume.Model.State.ItemSlotState Object)
    : IMimirBsonDocument(Address) { }
