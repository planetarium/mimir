using Mimir.MongoDB.Bson;

namespace Mimir.MongoDB.Tests.Bson;

public class BalanceDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new BalanceDocument(
            default,
            default,
            "0.00"
        );
        return Verify(docs.ToJson());
    }
}
