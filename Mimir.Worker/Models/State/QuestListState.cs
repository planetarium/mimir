using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.Quest;

namespace Mimir.Worker.Models;

public class QuestListState : IBencodable
{
    public QuestList Object;

    public QuestListState(QuestList questList)
    {
        Object = questList;
    }

    public IValue Bencoded => Object.Serialize();
}
