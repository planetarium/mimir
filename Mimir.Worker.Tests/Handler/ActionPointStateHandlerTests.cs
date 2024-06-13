using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;

namespace Mimir.Worker.Tests.Handler;

public class ActionPointStateHandlerTests
{
    private readonly ActionPointStateHandler _handler = new();

    [Theory]
    [InlineData(0)]
    [InlineData(120)]
    public void ConvertToStateData(int actionPoint)
    {
        var address = new PrivateKey().Address;
        var context = new StateDiffContext
        {
            Address = address,
            RawState = new Integer(actionPoint),
        };
        var stateData = _handler.ConvertToStateData(context);

        Assert.IsType<ActionPointState>(stateData.State);
        var dataState = (ActionPointState)stateData.State;
        Assert.Equal(address, dataState.address);
        Assert.Equal(actionPoint, dataState.Object);
    }
}
