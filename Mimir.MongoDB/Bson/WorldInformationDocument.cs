using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model;
using Nekoyume.Model.State;

namespace Mimir.MongoDB.Bson;

public record WorldInformationDocument : IMimirBsonDocument
{
    public Address Address { get; init; }
    public IDictionary<int, WorldInformation.World> Object { get; init; }
    private WorldInformation WorldInformationStateObject { get; init; }

    public WorldInformationDocument(
        Address address,
        WorldInformation worldInformation)
    {
        Address = address;
        Object = ((Dictionary)worldInformation.Serialize()).ToDictionary(
            kv => kv.Key.ToInteger(),
            kv => new WorldInformation.World((Dictionary)kv.Value));
        WorldInformationStateObject = worldInformation;
    }

    public IValue Bencoded => WorldInformationStateObject.Serialize();
}
