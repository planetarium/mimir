using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model;
using Nekoyume.Model.State;

namespace Mimir.MongoDB.Bson;

public class WorldInformationState : IBencodable
{
    public IDictionary<int, WorldInformation.World> Object;
    private WorldInformation WorldInformationStateObject;

    public WorldInformationState(WorldInformation worldInformation)
    {
        Object = ((Dictionary)worldInformation.Serialize()).ToDictionary(
            (KeyValuePair<IKey, IValue> kv) => kv.Key.ToInteger(),
            (KeyValuePair<IKey, IValue> kv) => new WorldInformation.World((Dictionary)kv.Value)
        );

        WorldInformationStateObject = worldInformation;
    }

    public IValue Bencoded => WorldInformationStateObject.Serialize();
}
