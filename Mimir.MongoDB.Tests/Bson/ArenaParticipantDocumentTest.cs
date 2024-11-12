using Lib9c.Models.Arena;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class ArenaParticipantDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new ArenaParticipantDocument(
            default,
            default,
            new ArenaParticipant(TestDataHelpers.LoadState("ArenaParticipant.bin"))
        );
        return Verify(docs.ToJson());
    }
}
