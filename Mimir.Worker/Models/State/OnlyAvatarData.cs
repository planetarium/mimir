using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class OnlyAvatarData : BaseData
{
    public Address Address { get; }

    public AvatarState State { get; }

    public OnlyAvatarData(Address address, AvatarState avatarState)
    {
        Address = address;
        State = avatarState;
    }
}
