using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.StateDocumentConverter;

namespace Mimir.Worker.Tests.StateDocumentConverter;

public class AllRuneStateDocumentConverterTests
{
    private readonly AllRuneStateDocumentConverter _converter = new();

    [Theory]
    [InlineData(0, 0)]
    [InlineData(999, 999)]
    public void ConvertToStateData(int runeId, int level)
    {
        var address = new PrivateKey().Address;
        var state = new Nekoyume.Model.State.AllRuneState(runeId, level);
        var bencoded = state.Serialize();
        var context = new AddressStatePair
        {
            Address = address,
            RawState = bencoded,
        };
        var doc = _converter.ConvertToDocument(context);
        Assert.IsType<AllRuneDocument>(doc);
        var allRuneDoc = (AllRuneDocument)doc;
        Assert.Equal(address, allRuneDoc.Address);
        Assert.Equal(bencoded, allRuneDoc.Object.Bencoded);
    }
}
