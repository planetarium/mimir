using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

public record RuneSlotDocument(Lib9c.Models.States.RuneSlotState Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Bencoded;
}
