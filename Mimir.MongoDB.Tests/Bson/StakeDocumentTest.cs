using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Extensions;
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
            new StakeState(TestDataHelpers.LoadState("StakeState.bin")));
        return Verify(docs.ToJson());
    }
}
