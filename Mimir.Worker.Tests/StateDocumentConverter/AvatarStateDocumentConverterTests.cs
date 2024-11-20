using Bencodex;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.StateDocumentConverter;
using Mimir.Worker.Tests.Fixtures.States;

namespace Mimir.Worker.Tests.StateDocumentConverter;

public class AvatarStateDocumentConverterTests
{
    private readonly Codec Codec = new();
    private readonly AvatarStateDocumentConverter _converter = new();

    [Fact]
    public void ConvertToStateData()
    {
        var address = new Address("4b4eccd6c6b17fe8d4312a0d2fadb0c93ad5a7ba");
        var rawState = StateReader.ReadState("avatarState");
        var context = new AddressStatePair()
        {
            Address = address,
            RawState = rawState,
        };
        var state = _converter.ConvertToDocument(context);

        Assert.IsType<AvatarDocument>(state);
    }
}
