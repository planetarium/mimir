using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class ActionPointState(int value) : IBencodable
{
    public int Object { get; set; } = value;

    public IValue Bencoded => Object.Serialize();
}
