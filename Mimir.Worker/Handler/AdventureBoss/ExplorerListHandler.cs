// using Bencodex.Types;
// using Mimir.Worker.Models;
// using Mimir.Worker.Models.State.AdventureBoss;
// using Mimir.Worker.Services;
// using Nekoyume.Model.AdventureBoss;

// namespace Mimir.Worker.Handler.AdventureBoss;

// public class ExplorerListHandler : IStateHandler<StateData>
// {
//     public StateData ConvertToStateData(StateDiffContext context) =>
//         new(context.Address, ConvertToState(context.RawState));

//     public async Task StoreStateData(IMongoDbService store, StateData stateData) =>
//         await store.UpsertStateDataAsyncWithLinkAvatar(stateData);

//     private static ExplorerListState ConvertToState(IValue state)
//     {
//         if (state is not List list)
//         {
//             throw new InvalidCastException(
//                 $"{nameof(state)} Invalid state type. Expected {nameof(List)}, got {state.GetType().Name}."
//             );
//         }

//         var explorerList = new ExplorerList(list);
//         return new ExplorerListState(explorerList);
//     }
// }
