using Bencodex;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;

namespace Mimir.Worker.Tests.Handler;

public class AgentStateHandlerTests
{
    private static readonly Codec Codec = new();
    private readonly AgentStateHandler _handler = new();

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void ConvertToStateData(int avatarCount)
    {
        var address = new PrivateKey().Address;
        var agentState = new Nekoyume.Model.State.AgentState(address);
        for (var i = 0; i < avatarCount; i++)
        {
            agentState.avatarAddresses.Add(i, new PrivateKey().Address);
        }

        var context = new StateDiffContext
        {
            Address = address,
            RawState = agentState.SerializeList(),
        };
        var stateData = _handler.ConvertToStateData(context);

        Assert.IsType<AgentState>(stateData.State);
        var dataState = (AgentState)stateData.State;
        Assert.Equal(agentState.address, dataState.Object.address);
        Assert.Equal(agentState.avatarAddresses.Count, dataState.Object.avatarAddresses.Count);
        foreach (var (key, value) in agentState.avatarAddresses)
        {
            Assert.True(dataState.Object.avatarAddresses.ContainsKey(key));
            Assert.Equal(value, dataState.Object.avatarAddresses[key]);
        }

        Assert.Equal(agentState.MonsterCollectionRound, dataState.Object.MonsterCollectionRound);
        Assert.Equal(agentState.Version, dataState.Object.Version);
    }
}
