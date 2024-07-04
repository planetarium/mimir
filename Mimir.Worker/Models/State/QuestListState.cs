using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.Quest;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class QuestListState : State
{
    public QuestList Object;

    public QuestListState(Address address, QuestList questList)
        : base(address)
    {
        Object = questList;
    }

    public override IValue Serialize()
    {
        return Object.Serialize();
    }
}
