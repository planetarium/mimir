using Lib9c.Models.Market;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Extensions;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class ProductsStateDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new ProductsStateDocument(
            default,
            new ProductsState(TestDataHelpers.LoadState("ProductsState.bin")),
            default);
        return Verify(docs.ToJson());
    }
}
