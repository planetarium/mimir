using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class WorldInformationState : State
{
    public IDictionary<int, WorldInformation.World> Object;
    private WorldInformation WorldInformationStateObject;

    public WorldInformationState(Address address, WorldInformation worldInformation)
        : base(address)
    {
        Object = ((Dictionary)worldInformation.Serialize()).ToDictionary(
            (KeyValuePair<IKey, IValue> kv) => kv.Key.ToInteger(),
            (KeyValuePair<IKey, IValue> kv) => new WorldInformation.World((Dictionary)kv.Value)
        );

        WorldInformationStateObject = worldInformation;
    }

    public override IValue Serialize()
    {
        return WorldInformationStateObject.Serialize();
    }
}
