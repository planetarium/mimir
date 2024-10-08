using Lib9c.Models.AttachmentActionResults;
using Lib9c.Models.Items;
using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Extensions;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class CombinationSlotStateDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new CombinationSlotStateDocument(
            default,
            default,
            0,
            new CombinationSlotState(TestDataHelpers.LoadState("CombinationSlotState.bin"))
        );

        return Verify(docs.ToJson());
    }
    
    [Fact]
    public Task JsonSnapshot_WithPetId()
    {
        var docs = new CombinationSlotStateDocument(
            default,
            default,
            0,
            new CombinationSlotState
            {
                UnlockStage = 1,
                UnlockBlockIndex = 100,
                StartBlockIndex = 0,
                PetId = 1,
            }
        );

        return Verify(docs.ToJson());
    }
    
    [Fact]
    public Task JsonSnapshot_WithResult()
    {
        var docs = new CombinationSlotStateDocument(
            default,
            default,
            0,
            new CombinationSlotState
            {
                UnlockStage = 1,
                UnlockBlockIndex = 100,
                StartBlockIndex = 0,
                Result = new DailyReward2Result
                {
                    Materials = new Dictionary<Material, int>(),
                    Id = Guid.Empty,
                    TypeId = "dailyReward.dailyRewardResult",
                    TradableFungibleItemCount = 0,
                },
            }
        );

        return Verify(docs.ToJson());
    }
}
