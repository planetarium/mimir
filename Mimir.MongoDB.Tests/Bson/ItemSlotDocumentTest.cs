using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Extensions;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class ItemSlotDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new ItemSlotDocument(
            default,
            new ItemSlotState(TestDataHelpers.LoadState("ItemSlotState.bin")));
        return Verify(docs.ToJson());
    }
}
