using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Nekoyume.Model.Quest;

namespace Mimir.Worker.Handler;

public class QuestListStateHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context)
    {
        var questList = ConvertToState(context.RawState);
        return new StateData(context.Address, new QuestListState(context.Address, questList));
    }

    private QuestList ConvertToState(IValue state)
    {
        if (state is Dictionary dictionary)
        {
            return new QuestList(dictionary);
        }
        else if (state is List alist)
        {
            return new QuestList(alist);
        }
        else
        {
            throw new ArgumentException(
                "Invalid state type. Expected Dictionary or List.",
                nameof(state)
            );
        }
    }
}
