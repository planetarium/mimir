using Bencodex;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.StateDocumentConverter;

namespace Mimir.Worker.Tests.StateDocumentConverter;

public class AvatarStateDocumentConverterTests
{
    private readonly Codec Codec = new();
    private readonly AvatarStateDocumentConverter _converter = new();

    [Fact]
    public void ConvertToStateData()
    {
        var address = new Address("4b4eccd6c6b17fe8d4312a0d2fadb0c93ad5a7ba");
        var rawState = TestHelpers.ReadTestData("avatarState.txt");
        var context = new AddressStatePair()
        {
            Address = address,
            RawState = Codec.Decode(Convert.FromHexString(rawState)),
        };
        var state = _converter.ConvertToDocument(context);

        Assert.IsType<AvatarDocument>(state);
    }
}
