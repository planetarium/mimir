using Mimir.MongoDB.Bson;

namespace Mimir.MongoDB.Tests.Bson;

public class PledgeDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new PledgeDocument(
            default,
            default,
            default,
            false,
            4);
        return Verify(docs.ToJson());
    }
}
