using Bencodex;
using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

public class RaiderState(Nekoyume.Model.State.RaiderState raiderState) : IBencodable
{
    public Nekoyume.Model.State.RaiderState Object { get; set; } = raiderState;
    public IValue Bencoded => Object.Serialize();
}
