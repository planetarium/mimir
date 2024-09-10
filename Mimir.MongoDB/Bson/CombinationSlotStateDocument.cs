using Lib9c.Models.States;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record CombinationSlotStateDocument(
    Address Address,
    Address AvatarAddress,
    int SlotIndex,
    CombinationSlotState Object
) : MimirBsonDocument(Address) { }
