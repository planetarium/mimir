// using Bencodex.Types;
// using Mimir.Worker.Models;
// using Mimir.Worker.Models.State.AdventureBoss;
// using Mimir.Worker.Services;
// using Nekoyume.Model.AdventureBoss;

// namespace Mimir.Worker.Handler.AdventureBoss;

// public class BountyBoardHandler : IStateHandler<StateData>
// {
//     public StateData ConvertToStateData(StateDiffContext context) =>
//         new(context.Address, ConvertToState(context.RawState));

//     public async Task StoreStateData(MongoDbService store, StateData stateData) =>
//         await store.UpsertStateDataAsyncWithLinkAvatar(stateData);

//     private static BountyBoardState ConvertToState(IValue state)
//     {
//         if (state is not List list)
//         {
//             throw new InvalidCastException(
//                 $"{nameof(state)} Invalid state type. Expected {nameof(List)}, got {state.GetType().Name}."
//             );
//         }

//         var bountyBoard = new BountyBoard(list);
//         return new BountyBoardState(bountyBoard);
//     }
// }
