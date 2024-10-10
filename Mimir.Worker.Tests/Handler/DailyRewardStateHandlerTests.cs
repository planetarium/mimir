using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Handler;

namespace Mimir.Worker.Tests.Handler;

public class DailyRewardStateHandlerTests
{
    private readonly DailyRewardStateHandler _handler = new();

    [Theory]
    [InlineData(0)]
    [InlineData(120)]
    public void ConvertToStateData(int dailyRewardReceivedBlockIndex)
    {
        var address = new PrivateKey().Address;
        var context = new StateDiffContext
        {
            Address = address,
            RawState = new Integer(dailyRewardReceivedBlockIndex),
        };
        var doc = _handler.ConvertToDocument(context);

        Assert.IsType<DailyRewardDocument>(doc);
        var dataState = (DailyRewardDocument)doc;
        Assert.Equal(address, dataState.Address);
        Assert.Equal(dailyRewardReceivedBlockIndex, dataState.Object);
    }
}
