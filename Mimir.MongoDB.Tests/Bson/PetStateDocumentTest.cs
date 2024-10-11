using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Extensions;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class PetStateDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new PetStateDocument(
            default,
            new PetState(TestDataHelpers.LoadState("PetState.bin")));
        return Verify(docs.ToJson());
    }
}