using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class StakeDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new StakeDocument(
            default,
            default,
            default,
            new StakeState(TestDataHelpers.LoadState("StakeState.bin")),
            0);
        return Verify(docs.ToJson());
    }
}
