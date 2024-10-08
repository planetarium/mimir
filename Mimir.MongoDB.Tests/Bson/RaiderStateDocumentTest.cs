using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Extensions;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class RaiderStateDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new RaiderStateDocument(
            default,
            new RaiderState(TestDataHelpers.LoadState("RaiderState.bin")));
        return Verify(docs.ToJson());
    }
}
