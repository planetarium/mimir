using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Json.Extensions;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class AgentDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new AgentDocument(
            default,
            new AgentState(TestDataHelpers.LoadState("Agent.bin"))
        );
        return Verify(docs.ToJson());
    }
}
