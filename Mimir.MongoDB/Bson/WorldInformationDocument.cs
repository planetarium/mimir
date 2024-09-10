using Bencodex.Types;
using Libplanet.Crypto;
using Lib9c.Models.Extensions;
using Nekoyume.Model;

namespace Mimir.MongoDB.Bson;

public record WorldInformationDocument : MimirBsonDocument
{
    public IDictionary<int, WorldInformation.World> Object { get; init; }

    public WorldInformationDocument(Address Address, WorldInformation worldInformation)
        : base(Address)
    {
        Object = ((Dictionary)worldInformation.Serialize()).ToDictionary(
            kv => kv.Key.ToInteger(),
            kv => new WorldInformation.World((Dictionary)kv.Value)
        );
    }
}
