using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class WorldBossStateDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new WorldBossStateDocument(
            default,
            default,
            default,
            new WorldBossState(TestDataHelpers.LoadState("WorldBossState.bin")));
        return Verify(docs.ToJson());
    }
}
