namespace Mimir.Worker.Tests.Handler;

using Bencodex;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;
using Xunit;

public class LegacyAccountHandlerTests
{
    private readonly Codec Codec = new();
    private readonly LegacyAccountHandler _handler = new LegacyAccountHandler();

    [Fact]
    public void ConvertToSheetStateData()
    {
        var address = new Address("8712b28bA68e24CAB6B460298f27B17Ef1A687d6");
        var rawState = TestHelpers.ReadTestData("collectionSheet.txt");
        var context = new StateDiffContext()
        {
            Address = address,
            RawState = Codec.Decode(Convert.FromHexString(rawState)),
        };
        var stateData = _handler.ConvertToStateData(context);

        Assert.IsType<SheetState>(stateData.State);
        Assert.Equal(stateData.State.address, address);
    }
}
