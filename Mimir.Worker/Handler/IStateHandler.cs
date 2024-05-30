using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;

namespace Mimir.Worker.Handler;

public interface IStateHandler<T> where T : StateData
{
    T ConvertToStateData(StateDiffContext context);
}
