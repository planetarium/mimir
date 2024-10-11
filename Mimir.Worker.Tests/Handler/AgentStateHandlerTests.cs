using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Handler;

namespace Mimir.Worker.Tests.Handler;

public class AgentStateHandlerTests
{
    private readonly AgentStateHandler _handler = new();

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
        var context = new StateDiffContext
        {
            Address = address,
            RawState = bencoded,
        };
        var doc = _handler.ConvertToDocument(context);
        Assert.IsType<AgentDocument>(doc);
        var agentDoc = (AgentDocument)doc;
        Assert.Equal(address, agentDoc.Address);
        Assert.Equal(bencoded, agentDoc.Object.Bencoded);
    }
}
