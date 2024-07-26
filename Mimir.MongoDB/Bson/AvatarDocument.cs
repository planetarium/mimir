using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

public record AvatarDocument(Lib9c.Models.States.AvatarState Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Bencoded;
}
