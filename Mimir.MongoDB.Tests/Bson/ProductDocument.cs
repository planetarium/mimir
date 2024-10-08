using Lib9c.Models.Market;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Extensions;
using Mimir.MongoDB.Tests.TestDatas;
using ItemProduct = Lib9c.Models.Market.ItemProduct;

namespace Mimir.MongoDB.Tests.Bson;

public class ProductDocumentTest
{
    [Fact]
    public Task JsonSnapshot_WithFavProduct()
    {
        var docs = new ProductDocument(
            default,
            default,
            default,
            new FavProduct(TestDataHelpers.LoadState("FavProduct.bin")));
        return Verify(docs.ToJson());
    }

    [Fact]
    public Task JsonSnapshot_WithItemProduct()
    {
        var docs = new ProductDocument(
            default,
            default,
            default,
            new ItemProduct(TestDataHelpers.LoadState("ItemProduct.bin")));
        return Verify(docs.ToJson());
    }
}
