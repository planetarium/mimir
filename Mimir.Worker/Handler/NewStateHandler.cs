using Bencodex.Types;
using Mimir.Models;
using Nekoyume.Model.State;

namespace Mimir.Worker.Handler;

public class NewStateHandler
{
    public StateModel ConvertToStateData(StateDiffContext context)
    {
        IValue state = context.RawState;
        string stateTypeName;

        if (state is List alist)
        {
            stateTypeName = alist[1].ToDotnetString();
        }
        else if (state is Dictionary dictionary)
        {
            stateTypeName = dictionary["StateTypeName"].ToDotnetString();
        }
        else
        {
            throw new InvalidCastException(
                $"{nameof(state)} Invalid state type. Expected Dictionary or List."
            );
        }

        if (string.IsNullOrEmpty(stateTypeName))
        {
            throw new InvalidOperationException("StateTypeName is null or empty.");
        }

        Type stateType = FindStateType(stateTypeName);
        if (stateType == null)
        {
            throw new InvalidOperationException($"State type '{stateTypeName}' not found.");
        }

        return (StateModel)Activator.CreateInstance(stateType, state);
    }

    private Type FindStateType(string stateTypeName)
    {
        var stateModelType = typeof(StateModel);
        var assembly = stateModelType.Assembly;
        var stateTypes = assembly
            .GetTypes()
            .Where(t => t.Namespace == stateModelType.Namespace && t.IsSubclassOf(stateModelType));

        return stateTypes.FirstOrDefault(t => t.Name == stateTypeName);
    }
}
