using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Extensions;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class CollectionDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new CollectionDocument(
            default,
            new CollectionState(TestDataHelpers.LoadState("Collection.bin"))
        );
        return Verify(docs.ToJson());
    }
}
