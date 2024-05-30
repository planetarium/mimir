using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Nekoyume.Model.Quest;

namespace Mimir.Worker.Handler;

public class QuestListStateHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(Address address, IValue rawState)
    {
        var questList = ConvertToState(rawState);
        return new StateData(address, new QuestListState(address, questList));
    }

    public StateData ConvertToStateData(Address address, string rawState)
    {
        Codec Codec = new();
        var state = Codec.Decode(Convert.FromHexString(rawState));
        var questList = ConvertToState(state);

        return new StateData(address, new QuestListState(address, questList));
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
