// using Bencodex;
// using Bencodex.Types;
// using Mimir.Worker.Exceptions;
// using Mimir.Worker.Models;
// using Mimir.Worker.Models.State.AdventureBoss;
// using Mimir.Worker.Services;
// using Nekoyume.Model.AdventureBoss;

// namespace Mimir.Worker.Handler.AdventureBoss;

// public class ExploreBoardHandler : IStateHandler<StateData>
// {
//     public StateData ConvertToStateData(StateDiffContext context) =>
//         new(context.Address, ConvertToBencodable(context.RawState));

//     public async Task StoreStateData(MongoDbService store, StateData stateData) =>
//         await store.UpsertStateDataAsyncWithLinkAvatar(stateData);

//     private static IBencodable ConvertToBencodable(IValue state)
//     {
//         if (state is not List list)
//         {
//             throw new InvalidCastException(
//                 $"{nameof(state)} Invalid state type. Expected {nameof(List)}, got {state.GetType().Name}."
//             );
//         }

//         try
//         {
//             var exploreBoard = new ExploreBoard(list);
//             return new ExploreBoardState(exploreBoard);
//         }
//         catch
//         {
//             // ignore.
//         }

//         try
//         {
//             var explorer = new Explorer(list);
//             return new ExplorerState(explorer);
//         }
//         catch
//         {
//             // ignore.
//         }

//         throw new UnexpectedStateException([
//             typeof(ExploreBoard),
//             typeof(Explorer)
//         ]);
//     }
// }
