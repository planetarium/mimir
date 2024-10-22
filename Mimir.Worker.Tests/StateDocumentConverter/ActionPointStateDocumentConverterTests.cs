using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.StateDocumentConverter;

namespace Mimir.Worker.Tests.StateDocumentConverter;

public class ActionPointStateDocumentConverterTests
{
    private readonly ActionPointStateDocumentConverter _converter = new();

    [Theory]
    [InlineData(0)]
    [InlineData(120)]
    public void ConvertToStateData(int actionPoint)
    {
        var address = new PrivateKey().Address;
        var context = new AddressStatePair
        {
            Address = address,
            RawState = new Integer(actionPoint),
        };
        var doc = _converter.ConvertToDocument(context);

        Assert.IsType<ActionPointDocument>(doc);
        var dataState = (ActionPointDocument)doc;
        Assert.Equal(address, dataState.Address);
        Assert.Equal(actionPoint, dataState.Object);
    }
}
