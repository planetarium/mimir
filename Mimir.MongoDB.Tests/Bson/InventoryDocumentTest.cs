using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Tests.TestDatas;
using Inventory = Lib9c.Models.Items.Inventory;

namespace Mimir.MongoDB.Tests.Bson;

public class InventoryDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new InventoryDocument(
            default,
            new Inventory(TestDataHelpers.LoadState("Inventory.bin"))
        );
        return Verify(docs.ToJson());
    }
}
