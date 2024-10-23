using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.StateDocumentConverter;

namespace Mimir.Worker.Tests.StateDocumentConverter;

public class DailyRewardStateDocumentConverterTests
{
    private readonly DailyRewardStateDocumentConverter _converter = new();

    [Theory]
    [InlineData(0)]
    [InlineData(120)]
    public void ConvertToStateData(int dailyRewardReceivedBlockIndex)
    {
        var address = new PrivateKey().Address;
        var context = new AddressStatePair
        {
            Address = address,
            RawState = new Integer(dailyRewardReceivedBlockIndex),
        };
        var doc = _converter.ConvertToDocument(context);

        Assert.IsType<DailyRewardDocument>(doc);
        var dataState = (DailyRewardDocument)doc;
        Assert.Equal(address, dataState.Address);
        Assert.Equal(dailyRewardReceivedBlockIndex, dataState.Object);
    }
}
