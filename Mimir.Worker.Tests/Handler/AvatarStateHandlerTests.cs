using Bencodex;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Handler;

namespace Mimir.Worker.Tests.Handler;

public class AvatarStateHandlerTests
{
    private readonly Codec Codec = new();
    private readonly AvatarStateHandler _handler = new();

    [Fact]
    public void ConvertToStateData()
    {
        var address = new Address("4b4eccd6c6b17fe8d4312a0d2fadb0c93ad5a7ba");
        var rawState = TestHelpers.ReadTestData("avatarState.txt");
        var context = new StateDiffContext()
        {
            Address = address,
            RawState = Codec.Decode(Convert.FromHexString(rawState)),
        };
        var state = _handler.ConvertToState(context);

        Assert.IsType<AvatarState>(state);
    }
}
