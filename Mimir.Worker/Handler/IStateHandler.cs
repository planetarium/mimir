using Nekoyume.Model.State;

namespace Mimir.Worker.Handler;

public interface IStateHandler<T> where T : State
{
    T ConvertToState(string rawState);
}
