using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Handler;

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
        var state = _handler.ConvertToDocument(context);

        Assert.IsType<ActionPointDocument>(state);
        var dataState = (ActionPointDocument)state;
        Assert.Equal(actionPoint, dataState.Object);
    }
}
