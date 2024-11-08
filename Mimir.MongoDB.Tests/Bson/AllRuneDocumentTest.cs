using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class AllRuneDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new AllRuneDocument(
            default,
            default,
            new AllRuneState(TestDataHelpers.LoadState("AllRuneState.bin"))
        );
        return Verify(docs.ToJson());
    }
}
