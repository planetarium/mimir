// using Bencodex;
// using Bencodex.Types;
// using Libplanet.Crypto;
// using Mimir.Worker.Constants;
// using Mimir.Worker.Models;
// using Mimir.Worker.Services;
// using MongoDB.Bson;
// using MongoDB.Driver;
// using Nekoyume.Model.Quest;
// using Nekoyume.Model.State;

// namespace Mimir.Worker.Handler;

// public class QuestListStateHandler : IStateHandler<StateData>
// {
//     public StateData ConvertToStateData(StateDiffContext context)
//     {
//         var questList = ConvertToState(context.RawState);
//         return new StateData(context.Address, new QuestListState(questList));
//     }

//     private QuestList ConvertToState(IValue state)
//     {
//         if (state is Dictionary dictionary)
//         {
//             return new QuestList(dictionary);
//         }
//         else if (state is List alist)
//         {
//             return new QuestList(alist);
//         }
//         else
//         {
//             throw new InvalidCastException(
//                 $"{nameof(state)} Invalid state type. Expected Dictionary or List."
//             );
//         }
//     }
// }

