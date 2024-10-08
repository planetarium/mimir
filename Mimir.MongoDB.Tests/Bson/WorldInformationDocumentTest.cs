using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Extensions;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class WorldInformationDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new WorldInformationDocument(
            default,
            new WorldInformationState(TestDataHelpers.LoadState("WorldInformation.bin")));
        return Verify(docs.ToJson());
    }
}
