using Lib9c.Models.Arena;
using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class ArenaDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new ArenaDocument(
            default,
            default,
            0,
            0,
            new ArenaInformation(TestDataHelpers.LoadState("ArenaInformation.bin")),
            new ArenaScore(TestDataHelpers.LoadState("ArenaScore.bin")),
            new SimplifiedAvatarState(TestDataHelpers.LoadState("Avatar.bin"))
        );
        return Verify(docs.ToJson());
    }
}
