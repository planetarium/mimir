using Mimir.MongoDB.Bson;

namespace Mimir.MongoDB.Tests.Bson;

public class ActionPointDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new ActionPointDocument(default, 1);
        return Verify(docs.ToJson());
    }
}
