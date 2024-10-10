using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Handler;

namespace Mimir.Worker.Tests.Handler;

public class AllRuneStateHandlerTests
{
    private readonly AllRuneStateHandler _handler = new();

    [Theory]
    [InlineData(0, 0)]
    [InlineData(999, 999)]
    public void ConvertToStateData(int runeId, int level)
    {
        var address = new PrivateKey().Address;
        var state = new Nekoyume.Model.State.AllRuneState(runeId, level);
        var bencoded = state.Serialize();
        var context = new StateDiffContext
        {
            Address = address,
            RawState = bencoded,
        };
        var doc = _handler.ConvertToDocument(context);
        Assert.IsType<AllRuneDocument>(doc);
        var allRuneDoc = (AllRuneDocument)doc;
        Assert.Equal(address, allRuneDoc.Address);
        Assert.Equal(bencoded, allRuneDoc.Object.Bencoded);
    }
}
