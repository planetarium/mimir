using Bencodex;
using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

public class AvatarState(Lib9c.Models.States.AvatarState avatarState) : IBencodable
{
    public Lib9c.Models.States.AvatarState Object { get; } = avatarState;

    public IValue Bencoded => Object.Bencoded;
}
