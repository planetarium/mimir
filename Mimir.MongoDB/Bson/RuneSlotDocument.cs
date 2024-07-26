using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record RuneSlotDocument(
    Address Address,
    Lib9c.Models.States.RuneSlotState Object)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Bencoded;
}
