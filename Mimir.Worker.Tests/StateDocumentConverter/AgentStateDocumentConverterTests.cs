using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.StateDocumentConverter;

namespace Mimir.Worker.Tests.StateDocumentConverter;

public class AgentStateDocumentConverterTests
{
    private readonly AgentStateDocumentConverter _converter = new();

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void ConvertToStateData(int avatarCount)
    {
        var address = new PrivateKey().Address;
        var state = new Nekoyume.Model.State.AgentState(address);
        for (var i = 0; i < avatarCount; i++)
        {
            state.avatarAddresses.Add(i, new PrivateKey().Address);
        }

        var bencoded = state.SerializeList();
        var context = new AddressStatePair
        {
            Address = address,
            RawState = bencoded,
        };
        var doc = _converter.ConvertToDocument(context);
        Assert.IsType<AgentDocument>(doc);
        var agentDoc = (AgentDocument)doc;
        Assert.Equal(address, agentDoc.Address);
        Assert.Equal(bencoded, agentDoc.Object.Bencoded);
    }
}
