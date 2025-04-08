using Lib9c.Models.States;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Tests.TestDatas;

namespace Mimir.MongoDB.Tests.Bson;

public class AvatarDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var docs = new AvatarDocument(
            default,
            default,
            new AvatarState(TestDataHelpers.LoadState("Avatar.bin")),
            default,
            default
        );
        return Verify(docs.ToJson());
    }
}
