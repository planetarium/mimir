namespace Mimir.Worker.Tests.Handler;

using Bencodex;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Nekoyume.Model.State;
using Xunit;
using AvatarState = Mimir.Models.AvatarState;

public class NewStateHandlerTests
{
    private readonly Codec Codec = new();
    private readonly NewStateHandler _handler = new NewStateHandler();

    [Fact]
    public void ConvertToStateData()
    {
        var address = new Address("4b4eccd6c6b17fe8d4312a0d2fadb0c93ad5a7ba");
        var rawState = TestHelpers.ReadTestData("newAvatarState.txt");
        var context = new StateDiffContext()
        {
            Address = address,
            RawState = Codec.Decode(Convert.FromHexString(rawState)),
        };
        var stateData = _handler.ConvertToStateData(context);

        Assert.IsType<AvatarState>(stateData);
    }
}
