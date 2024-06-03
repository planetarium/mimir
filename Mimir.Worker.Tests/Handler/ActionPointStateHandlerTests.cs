using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;

namespace Mimir.Worker.Tests.Handler;

public class ActionPointStateHandlerTests
{
    private static readonly Codec Codec = new();
    private readonly ActionPointStateHandler _handler = new();

    [Theory]
    [InlineData(0)]
    [InlineData(120)]
    public void ConvertToStateData(int actionPoint)
    {
        var address = new PrivateKey().Address;
        var rawState = Convert.ToHexString(Codec.Encode(new Integer(actionPoint)));
        var stateData = _handler.ConvertToStateData(address, rawState);

        Assert.IsType<ActionPointState>(stateData.State);
        var actionPointState = (ActionPointState)stateData.State;
        Assert.Equal(address, actionPointState.address);
        Assert.Equal(actionPoint, actionPointState.Value);
    }
}
