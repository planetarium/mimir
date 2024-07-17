using Bencodex;
using Bencodex.Types;

namespace Mimir.Worker.Models;

public class AvatarState(Nekoyume.Model.State.AvatarState avatarState) : IBencodable
{
    public Nekoyume.Model.State.AvatarState Object { get; } = avatarState;

    public IValue Bencoded => Object.SerializeList();
}
