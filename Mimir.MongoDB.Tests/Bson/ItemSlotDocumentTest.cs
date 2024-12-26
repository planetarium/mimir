using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class ItemSlotDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new ItemSlotDocument(
            default,
            default,
            default,
            new ItemSlotState(TestDataHelpers.LoadState("ItemSlotState.bin"))
        );
        return Verify(docs.ToJson());
    }
}
