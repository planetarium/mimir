using Bencodex;
using Bencodex.Types;

namespace Mimir.Worker.Models;

public class ItemSlotState(Nekoyume.Model.State.ItemSlotState itemSlotState) : IBencodable
{
    public Nekoyume.Model.State.ItemSlotState Object { get; } = itemSlotState;

    public IValue Bencoded => Object.Serialize();
}
