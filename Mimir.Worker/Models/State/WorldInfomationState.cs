using Libplanet.Crypto;
using Nekoyume.Model;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class WorldInformationState : State
{
    public WorldInformation WorldInformation;

    public WorldInformationState(Address address, WorldInformation worldInformation)
        : base(address)
    {
        WorldInformation = worldInformation;
    }
}
