using Mimir.Worker.Models;
using Mimir.Worker.Services;

namespace Mimir.Worker.Handler;

public interface IStateHandler<T>
    where T : StateData
{
    T ConvertToStateData(StateDiffContext context);
    Task StoreStateData(DiffMongoDbService store, StateData stateData);
}
