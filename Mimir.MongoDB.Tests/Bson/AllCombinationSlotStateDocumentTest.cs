// using Lib9c.Models.States;
// using Mimir.MongoDB.Bson;
// using Mimir.MongoDB.Json.Extensions;
// using Mimir.MongoDB.Tests.TestDatas;
//
// namespace Mimir.MongoDB.Tests.Bson;
//
// public class AllCombinationSlotStateDocumentTest
// {
//     [Fact]
//     public Task JsonSnapshot()
//     {
//         var docs = new AllCombinationSlotStateDocument(
//             default,
//             new AllCombinationSlotState(TestDataHelpers.LoadState("AllCombinationSlotState.bin"))
//         );
//
//         return Verify(docs.ToJson());
//     }
// }
