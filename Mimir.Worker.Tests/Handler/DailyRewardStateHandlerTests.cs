using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;

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
        var state = _handler.ConvertToState(context);

        Assert.IsType<DailyRewardState>(state);
        var dataState = (DailyRewardState)state;
        Assert.Equal(dailyRewardReceivedBlockIndex, dataState.Object);
    }
}
