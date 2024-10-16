using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class RuneSlotDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new RuneSlotDocument(
            default,
            new RuneSlotState(TestDataHelpers.LoadState("RuneSlotState.bin")));
        return Verify(docs.ToJson());
    }
}
