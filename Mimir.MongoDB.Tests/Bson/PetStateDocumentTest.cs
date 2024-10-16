using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class PetStateDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new PetStateDocument(
            default,
            default,
            new PetState(TestDataHelpers.LoadState("PetState.bin")));
        return Verify(docs.ToJson());
    }
}
