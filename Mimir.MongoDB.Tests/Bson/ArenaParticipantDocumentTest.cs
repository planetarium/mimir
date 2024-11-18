using Lib9c.Models.Arena;
using Lib9c.Models.States;
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
            new ArenaParticipant(TestDataHelpers.LoadState("ArenaParticipant.bin")),
            default,
            default,
            SimplifiedAvatarState.FromAvatarState(
                new AvatarState(TestDataHelpers.LoadState("Avatar.bin"))));
        return Verify(docs.ToJson());
    }
}
