// using Bencodex.Types;
// using Libplanet.Crypto;
// using Lib9c.Models.AdventureBoss;
// using Mimir.Worker.Models;
// using Mimir.Worker.Models.State.AdventureBoss;
// using Mimir.Worker.Services;

// namespace Mimir.Worker.Handler.AdventureBoss;

// public class SeasonInfoHandler : IStateHandler<StateData>
// {
//     public StateData ConvertToStateData(StateDiffContext context) =>
//         new(context.Address, ConvertToState(context.Address, context.RawState));

//     public async Task StoreStateData(IMongoDbService store, StateData stateData) =>
//         await store.UpsertStateDataAsyncWithLinkAvatar(stateData);

//     private static SeasonInfoState ConvertToState(Address address, IValue state)
//     {
//         if (state is not List list)
//         {
//             throw new InvalidCastException(
//                 $"{nameof(state)} Invalid state type. Expected {nameof(List)}, got {state.GetType().Name}."
//             );
//         }

//         var seasonInfo = new Nekoyume.Model.AdventureBoss.SeasonInfo(list);
//         return new SeasonInfoState(new SeasonInfo(address, seasonInfo), seasonInfo.Bencoded);
//     }
// }
