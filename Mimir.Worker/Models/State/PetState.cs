using Bencodex;
using Bencodex.Types;
using NCPetState = Nekoyume.Model.State.PetState;

namespace Mimir.Worker.Models;

public class PetState : IBencodable
{
    public NCPetState Object;

    public PetState(NCPetState petState)
    {
        Object = petState;
    }

    public IValue Bencoded => Object.Serialize();
}
