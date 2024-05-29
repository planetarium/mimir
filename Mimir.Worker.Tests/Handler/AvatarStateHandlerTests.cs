namespace Mimir.Worker.Tests.Handler;

using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Nekoyume.Model.State;
using Xunit;

public class AvatarStateHandlerTests
{
    private readonly AvatarStateHandler _handler = new AvatarStateHandler();

    [Fact]
    public void ConvertToStateData()
    {
        var address = new Address("4b4eccd6c6b17fe8d4312a0d2fadb0c93ad5a7ba");
        var rawState = TestHelpers.ReadTestData("avatarState.txt");
        var stateData = _handler.ConvertToStateData(address, rawState);

        Assert.IsType<AvatarState>(stateData.State);
    }
}
