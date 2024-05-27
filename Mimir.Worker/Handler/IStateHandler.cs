using Bencodex.Types;
using Mimir.Worker.Models;

namespace Mimir.Worker.Handler;

public interface IStateHandler<T> where T : StateData
{
    T ConvertToStateData(string rawState);
    T ConvertToStateData(IValue rawState);
}
