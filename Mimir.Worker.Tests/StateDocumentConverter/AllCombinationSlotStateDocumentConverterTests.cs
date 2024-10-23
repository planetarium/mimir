using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.StateDocumentConverter;

namespace Mimir.Worker.Tests.StateDocumentConverter;

public class AllCombinationSlotStateDocumentConverterTests
{
    private readonly AllCombinationSlotStateDocumentConverter _converter = new();

    [Fact]
    public void ConvertToStateData()
    {
        var address = new PrivateKey().Address;
        var state = new Nekoyume.Model.State.AllCombinationSlotState();
        var bencoded = state.Serialize();
        var context = new AddressStatePair
        {
            Address = address,
            RawState = bencoded,
        };
        var doc = _converter.ConvertToDocument(context);
        Assert.IsType<AllCombinationSlotStateDocument>(doc);
        var combinationSlotStateDoc = (AllCombinationSlotStateDocument)doc;
        Assert.Equal(address, combinationSlotStateDoc.Address);
        Assert.Equal(bencoded, combinationSlotStateDoc.Object.Bencoded);
    }
}
