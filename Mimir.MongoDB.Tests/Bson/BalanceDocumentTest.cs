using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Extensions;

namespace Mimir.MongoDB.Tests.Bson;

public class BalanceDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new BalanceDocument(
            default,
            "0.00"
        );
        return Verify(docs.ToJson());
    }
}
