namespace Mimir.Worker.Tests.Handler;

using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;
using Xunit;

public class LegacyAccountHandlerTests
{
    private readonly LegacyAccountHandler _handler = new LegacyAccountHandler();

    [Fact]
    public void ConvertToSheetStateData()
    {
        var address = new Address("8712b28bA68e24CAB6B460298f27B17Ef1A687d6");
        var rawState = TestHelpers.ReadTestData("collectionSheet.txt");
        var stateData = _handler.ConvertToStateData(address, rawState);

        Assert.IsType<SheetState>(stateData.State);
        Assert.Equal(stateData.State.address, address);
    }
}
