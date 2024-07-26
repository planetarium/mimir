using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record AvatarDocument(Lib9c.Models.States.AvatarState Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Bencoded;
    public Address Address => Object.Address;
}
