using Bencodex;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Handler;

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
        var state = _handler.ConvertToState(context);

        Assert.IsType<AgentDocument>(state);
        var dataState = (AgentDocument)state;
        Assert.Equal(agentState.address, dataState.Object.Address);
        Assert.Equal(agentState.avatarAddresses.Count, dataState.Object.AvatarAddresses.Count);
        foreach (var (key, value) in agentState.avatarAddresses)
        {
            Assert.True(dataState.Object.AvatarAddresses.ContainsKey(key));
            Assert.Equal(value, dataState.Object.AvatarAddresses[key]);
        }

        Assert.Equal(agentState.MonsterCollectionRound, dataState.Object.MonsterCollectionRound);
        Assert.Equal(agentState.Version, dataState.Object.Version);
    }
}
