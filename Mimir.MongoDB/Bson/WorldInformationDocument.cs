using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model;
using Nekoyume.Model.State;

namespace Mimir.MongoDB.Bson;

public record WorldInformationDocument(Address Address) : IMimirBsonDocument(Address)
{
    public IDictionary<int, WorldInformation.World> Object { get; init; }

    public WorldInformationDocument(Address Address, WorldInformation worldInformation)
        : this(Address)
    {
        Object = ((Dictionary)worldInformation.Serialize()).ToDictionary(
            kv => kv.Key.ToInteger(),
            kv => new WorldInformation.World((Dictionary)kv.Value)
        );
    }
}
