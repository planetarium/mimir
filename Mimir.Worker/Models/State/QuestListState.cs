using Libplanet.Crypto;
using Nekoyume.Model.Quest;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public class QuestListState : State
{
    public QuestList QuestList;

    public QuestListState(Address address, QuestList questList)
        : base(address)
    {
        QuestList = questList;
    }
}
