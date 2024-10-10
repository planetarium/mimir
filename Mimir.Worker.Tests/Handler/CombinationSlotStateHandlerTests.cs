using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Handler;

namespace Mimir.Worker.Tests.Handler;

public class CombinationSlotStateHandlerTests
{
    private readonly CombinationSlotStateHandler _handler = new();

    [Fact]
    public void ConvertToStateData()
    {
        var address = new PrivateKey().Address;
        var state = new Nekoyume.Model.State.CombinationSlotState(address);
        var bencoded = state.Serialize();
        var context = new StateDiffContext
        {
            Address = address,
            RawState = bencoded,
        };
        var doc = _handler.ConvertToDocument(context);
        Assert.IsType<CombinationSlotStateDocument>(doc);
        var combinationSlotStateDoc = (CombinationSlotStateDocument)doc;
        Assert.Equal(address, combinationSlotStateDoc.Address);
        Assert.Equal(bencoded, combinationSlotStateDoc.Object.Bencoded);
    }
}
